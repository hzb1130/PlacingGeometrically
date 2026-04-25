using MelonLoader;
using HarmonyLib;
using UnityEngine;
using Il2Cpp;
using System.Collections.Generic;
using Il2CppTLD.Placement;

[assembly: MelonInfo(typeof(PlacingGeometrically.PlacingGeometricallyMain), "Placing Geometrically", "1.0.0", "hzb1130")]
[assembly: MelonGame("Hinterland", "TheLongDark")]

namespace PlacingGeometrically
{
    public class PlacingGeometricallyMain : MelonMod
    {
        private GameObject? paHUD = null;
        // 所有按键 → ButtonPrompt 映射（用于高亮）
        private readonly Dictionary<KeyCode, ButtonPrompt> buttonMap = new Dictionary<KeyCode, ButtonPrompt>();

        // 四个模式键的状态
        private bool gridModeOn = false;
        private bool snapXZOn = false;
        private bool yCollisionOn = false;
        private bool coordRotateSwitchOn = false;
        private Vector3 _extraOffset = Vector3.zero;
        private Vector3 _extraRotation = Vector3.zero;  // 新增
        private Vector3 _lockedPosition = Vector3.zero;
        private Vector3 _rotationCancel = Vector3.zero;  // 归零抵消量
        
        
        private PlayerManager? playerManager = null;

        public override void OnInitializeMelon()
        {
            Settings.OnLoad();
        }

        private void CreatePlacingHUD()
        {
            if (paHUD != null) return;
            if (!Settings.options.enableFineMode)
            {
                gridModeOn = false;
                snapXZOn = false;
                yCollisionOn = false;
                coordRotateSwitchOn = false;
                return;
            }

            paHUD = new GameObject("PlacingGeometrically_HUD");

            var placingHUD = InterfaceManager.GetPanel<Panel_HUD>().m_EquipItemPopup;
            GameObject bpButton = placingHUD.m_ButtonPromptRight.gameObject;

            float baseX = Settings.options.hudOffsetX;
            float baseY = Settings.options.hudOffsetY;

            const float colWidth = 110f;
            const float rowHeight = 80f;

            void AddButton(string label, KeyCode keyCode, int rowIndex, int colIndex)
            {
                GameObject go = NGUITools.AddChild(paHUD, bpButton);
                go.name = "PG_Button_" + keyCode;
                ButtonPrompt button = go.GetComponent<ButtonPrompt>();

                button.UpdatePromptLabel(label);
                button.m_KeyboardButtonLabel.text = keyCode.ToString();

                string keyText = keyCode.ToString();
                if (keyText.Length > 1)
                {
                    float extra = (keyText.Length - 1) * 0.4f;
                    button.m_KeyboardButtonSprite.transform.localScale = new Vector3(1f + extra, 1f, 1f);
                }

                float posX = baseX + colIndex * colWidth;
                float posY = baseY - rowIndex * rowHeight;
                go.transform.localPosition = new Vector3(posX, posY, 0);

                if (!buttonMap.ContainsKey(keyCode))
                    buttonMap.Add(keyCode, button);
            }

            // // 第一行：网格放置，吸附XZ
            // AddButton("网格放置", Settings.options.keyGridMode, 0, 0);
            // AddButton("吸附摆放",   Settings.options.keySnapItemXZ, 0, 1);
            // // 第二行：Y叠放，坐标/旋转切换
            // AddButton("高度叠放",    Settings.options.keyYCollisionStack, 1, 0);
            // AddButton("坐标/旋转切换", Settings.options.keyCoordRotateSwitch, 1, 1);
            // // 第三行：
            // AddButton("归零旋转", Settings.options.keyResetRotateZero, 2, 0);
            // if (Settings.options.enableXYZAdjust)
            // {
            //     // 第4行
            //     AddButton("X+", Settings.options.keyXPlus, 3, 0);
            //     AddButton("X-", Settings.options.keyXMinus, 3, 1);

            //     // 第5行
            //     AddButton("Y+", Settings.options.keyYPlus, 4, 0);
            //     AddButton("Y-", Settings.options.keyYMinus, 4, 1);

            //     // 第6行
            //     AddButton("Z+", Settings.options.keyZPlus, 5, 0);
            //     AddButton("Z-", Settings.options.keyZMinus, 5, 1);
            // }

            // Row 1: Grid placement, snap on XZ
            AddButton("Grid Placement", Settings.options.keyGridMode, 0, 0);
            AddButton("Snap Placement", Settings.options.keySnapItemXZ, 0, 1);

            // Row 2: Y stacking, coordinate/rotation toggle
            AddButton("Vertical Stacking", Settings.options.keyYCollisionStack, 1, 0);
            AddButton("Coordinate/Rotation Toggle", Settings.options.keyCoordRotateSwitch, 1, 1);

            // Row 3:
            AddButton("Reset Rotation", Settings.options.keyResetRotateZero, 2, 0);

            if (Settings.options.enableXYZAdjust)
            {
                // Row 4
                AddButton("X+", Settings.options.keyXPlus, 3, 0);
                AddButton("X-", Settings.options.keyXMinus, 3, 1);

                // Row 5
                AddButton("Y+", Settings.options.keyYPlus, 4, 0);
                AddButton("Y-", Settings.options.keyYMinus, 4, 1);

                // Row 6
                AddButton("Z+", Settings.options.keyZPlus, 5, 0);
                AddButton("Z-", Settings.options.keyZMinus, 5, 1);
            }

        }

        private void DestroyPlacingHUD()
        {
            if (paHUD != null)
            {
                UnityEngine.Object.Destroy(paHUD);
                paHUD = null;
                buttonMap.Clear();
                // 重置状态
                // gridModeOn = false;
                // snapXZOn = false;
                // yCollisionOn = false;
                _extraOffset = Vector3.zero;
                _extraRotation = Vector3.zero;
                _lockedPosition = Vector3.zero;
                _rotationCancel = Vector3.zero;
                playerManager = null;
                coordRotateSwitchOn = false;
            }
        }

        // ================= 按键高亮 / 模式切换 =================
        public override void OnUpdate()
        {
            if (!Settings.options.enableFineMode && paHUD != null)
            {
                DestroyPlacingHUD();
                return;
            }

            if (paHUD == null || !Settings.options.enableFineMode)
                return;

            // ---- 模式切换按键检测 ----
            if (Input.GetKeyDown(Settings.options.keyGridMode))
            {
                gridModeOn = !gridModeOn;
                if (gridModeOn) snapXZOn = false; // 网格开启时关闭吸附
            }
            if (Input.GetKeyDown(Settings.options.keySnapItemXZ))
            {
                snapXZOn = !snapXZOn;
                if (snapXZOn) gridModeOn = false; // 吸附开启时关闭网格
            }
            if (Input.GetKeyDown(Settings.options.keyYCollisionStack))
                yCollisionOn = !yCollisionOn;
            if (Input.GetKeyDown(Settings.options.keyCoordRotateSwitch))
            {
                coordRotateSwitchOn = !coordRotateSwitchOn;
               if (coordRotateSwitchOn && playerManager != null)
                {
                    GameObject obj = playerManager.GetObjectToPlace();
                    if (obj != null)
                    {
                        _lockedPosition = obj.transform.position;
                        _extraRotation = obj.transform.eulerAngles;
                        _rotationCancel = Vector3.zero;   //清空归零状态
                    }
                }
                // 退出旋转模式
            }
            // ---- 颜色刷新 ----
            var keys = new List<KeyCode>(buttonMap.Keys);
            foreach (var key in keys)
            {
                if (!buttonMap.TryGetValue(key, out var button)) continue;

                Color targetColor;

                // 判断是否为模式键
                bool isModeKey = key == Settings.options.keyGridMode ||
                                 key == Settings.options.keySnapItemXZ ||
                                 key == Settings.options.keyYCollisionStack ||
                                 key == Settings.options.keyCoordRotateSwitch;

                if (isModeKey)
                {
                    // 模式键根据内部状态决定颜色
                    bool state = false;
                    if (key == Settings.options.keyGridMode) state = gridModeOn;
                    else if (key == Settings.options.keySnapItemXZ) state = snapXZOn;
                    else if (key == Settings.options.keyYCollisionStack) state = yCollisionOn;
                    else if (key == Settings.options.keyCoordRotateSwitch) state = coordRotateSwitchOn;

                    targetColor = state ? new Color32(255, 140, 0, 255) : Color.white;
                }
                else
                {
                    // 微调键和归零键：按住时高亮
                    targetColor = Input.GetKey(key) ? new Color32(255, 140, 0, 255) : Color.white;
                }

                button.m_KeyboardButtonLabel.color = targetColor;
                button.m_KeyboardButtonSprite.color = targetColor;
            }
        }
        private static GearItem? FindNearbyGear(Vector3 pos, float radius = 0.6f)
        {
            Collider[] cols = Physics.OverlapSphere(pos, radius);

            float bestDist = float.MaxValue;
            GearItem? best = null;

            foreach (var col in cols)
            {
                var gear = col.GetComponentInParent<GearItem>();
                if (gear == null) continue;

                float d = Vector3.Distance(pos, gear.transform.position);

                if (d < bestDist)
                {
                    bestDist = d;
                    best = gear;
                }
            }

            return best;
        }

        // ================= Patch 控制 HUD 生命周期 =================

        [HarmonyPatch(typeof(PlayerManager), "StartPlaceMesh",
            new[] { typeof(GameObject), typeof(float), typeof(PlaceMeshFlags), typeof(PlaceMeshRules) })]
        internal static class Patch_StartPlaceMesh
        {
            private static void Postfix(PlayerManager __instance)
            {
                var mod = Melon<PlacingGeometricallyMain>.Instance;
                mod.playerManager = __instance;
                mod.CreatePlacingHUD();
                GameObject obj = __instance.GetObjectToPlace();
                if (obj != null)
                {
                    mod._extraRotation = obj.transform.eulerAngles;
                }
                if (mod.paHUD != null && !Utils.IsGamepadActive())
                    mod.paHUD.SetActive(true);
            }
        }

        [HarmonyPatch(typeof(PlayerManager), "ExitMeshPlacement")]
        internal static class Patch_ExitMeshPlacement
        {
            private static void Postfix()
            {
                Melon<PlacingGeometricallyMain>.Instance.DestroyPlacingHUD();
            }
        }

        [HarmonyPatch(typeof(PlayerManager), "CancelPlaceMesh")]
        internal static class Patch_CancelPlaceMesh
        {
            private static void Postfix()
            {
                Melon<PlacingGeometricallyMain>.Instance.DestroyPlacingHUD();
            }
        }

        [HarmonyPatch(typeof(PlayerManager), nameof(PlayerManager.DoPositionCheck))]
        internal static class Patch_DoPositionCheck
        {
            private static void Postfix(PlayerManager __instance, ref MeshLocationCategory __result)
            {
                if (!Settings.options.enableAnyPlace) return;
                __result = MeshLocationCategory.Valid;

                var mod = Melon<PlacingGeometricallyMain>.Instance;
                GameObject obj = __instance.GetObjectToPlace();
                if (obj == null) return;

                float step = 360f / Settings.options.rotateDivide;
                float moveStep = Settings.options.fineMoveStep;

                // ====== 归零键：记录当前旋转作为抵消基准 ======
                if (Input.GetKeyDown(Settings.options.keyResetRotateZero))
                {
                    if (mod.coordRotateSwitchOn)
                    {
                        mod._rotationCancel = mod._extraRotation;
                    }
                    else
                    {
                        mod._rotationCancel = obj.transform.eulerAngles;
                    }
                }

                bool xyzEnabled = Settings.options.enableXYZAdjust;

                // ====== 旋转模式 ======
                if (mod.coordRotateSwitchOn)
                {
                    // 锁定坐标
                    obj.transform.position = mod._lockedPosition;
                    if (xyzEnabled)
                    {
                        // 累积旋转偏移
                        if (Input.GetKeyDown(Settings.options.keyXPlus))   mod._extraRotation.x += step;
                        if (Input.GetKeyDown(Settings.options.keyXMinus))  mod._extraRotation.x -= step;
                        if (Input.GetKeyDown(Settings.options.keyYPlus))   mod._extraRotation.y += step;
                        if (Input.GetKeyDown(Settings.options.keyYMinus))  mod._extraRotation.y -= step;
                        if (Input.GetKeyDown(Settings.options.keyZPlus))   mod._extraRotation.z += step;
                        if (Input.GetKeyDown(Settings.options.keyZMinus))  mod._extraRotation.z -= step;
                    }
                
                    // 先应用基础旋转（由 _extraRotation 决定）
                    obj.transform.rotation = Quaternion.Euler(mod._extraRotation);

                    // 再应用归零抵消（同坐标模式逻辑）
                    if (mod._rotationCancel != Vector3.zero)
                    {
                        Quaternion cancelRot = Quaternion.Euler(mod._rotationCancel);
                        obj.transform.rotation = Quaternion.Inverse(cancelRot) * obj.transform.rotation;
                    }

                    return;
                }

                // ====== 坐标模式 ======
                // 坐标微调累加
                if (xyzEnabled)
                {
                    if (Input.GetKeyDown(Settings.options.keyXPlus))   mod._extraOffset.x += moveStep;
                    if (Input.GetKeyDown(Settings.options.keyXMinus))  mod._extraOffset.x -= moveStep;
                    if (Input.GetKeyDown(Settings.options.keyYPlus))   mod._extraOffset.y += moveStep;
                    if (Input.GetKeyDown(Settings.options.keyYMinus))  mod._extraOffset.y -= moveStep;
                    if (Input.GetKeyDown(Settings.options.keyZPlus))   mod._extraOffset.z += moveStep;
                    if (Input.GetKeyDown(Settings.options.keyZMinus))  mod._extraOffset.z -= moveStep;
                }
                

                // 位置偏移（原有逻辑不变）
                if (mod.gridModeOn)
                {
                    Vector3 pos = obj.transform.position;
                    pos.x = Mathf.Round(pos.x) + mod._extraOffset.x;
                    pos.z = Mathf.Round(pos.z) + mod._extraOffset.z;
                    pos.y += mod._extraOffset.y;
                    obj.transform.position = pos;
                }
                else if (mod.snapXZOn)
                {
                    vp_FPSCamera cam = GameManager.GetVpFPSCamera();
                    Ray ray = new Ray(cam.transform.position, cam.transform.forward);
                    RaycastHit hit;

                    int layerMask = PlayerManager.GetLayerMaskForPlaceMeshRaycast();
                    layerMask |= 1 << vp_Layer.Gear;

                    GearItem? gear = null;

                    if (Physics.Raycast(ray, out hit, 6f, layerMask))
                    {
                        gear = hit.collider.GetComponentInParent<GearItem>();
                    }

                    if (gear == null)
                    {
                        gear = FindNearbyGear(obj.transform.position);
                    }

                    if (gear != null)
                    {
                        GameObject target = gear.gameObject;

                        obj.transform.position = target.transform.position + mod._extraOffset;
                        obj.transform.rotation = target.transform.rotation;
                    }
                }
                else
                {
                    obj.transform.position += mod._extraOffset;
                }

                // ===== Y叠放（叠加层）=====
                if (mod.yCollisionOn)
                {
                    vp_FPSCamera cam = GameManager.GetVpFPSCamera();
                    Ray ray = new Ray(cam.transform.position, cam.transform.forward);
                    RaycastHit hit;

                    int layerMask = 1 << vp_Layer.Gear;


                layerMask |= 1 << vp_Layer.Gear;

                GearItem? gear = null;

                if (Physics.Raycast(ray, out hit, 6f, layerMask))
                {
                    gear = hit.collider.GetComponentInParent<GearItem>();
                }

                if (gear == null)
                {
                    gear = FindNearbyGear(obj.transform.position);
                }

                if (gear != null && gear.gameObject != obj)
                {
                    GameObject target = gear.gameObject;

                    Collider targetCol = target.GetComponentInChildren<Collider>();

                    if (targetCol != null)
                    {
                        float targetTop = targetCol.bounds.max.y;

                        Vector3 pos = obj.transform.position;

                        float finalY = targetTop + mod._extraOffset.y;

                        obj.transform.position = new Vector3(
                            pos.x,
                            finalY,
                            pos.z
                        );
                    }
                }
                }
                else
                {
                    // 没开Y叠放 → 正常Y偏移
                    obj.transform.position += new Vector3(0, mod._extraOffset.y, 0);
                }

                // ====== 原版/坐标模式下应用旋转抵消（归零逻辑） ======
                // 如果 _rotationCancel 非零（即按过归零键），则应用抵消
                if (mod._rotationCancel != Vector3.zero)
                {
                    // 当前旋转 - 抵消基准 = 用户想要的旋转
                    Vector3 currentEuler = obj.transform.eulerAngles;
                    // 由于欧拉角减法可能有wrap问题，使用Quaternion相减更准确
                    Quaternion currentRot = obj.transform.rotation;
                    Quaternion cancelRot = Quaternion.Euler(mod._rotationCancel);
                    // 目标旋转 = cancelRot的逆 × currentRot  相当于 currentEuler - cancelEuler（在四元数空间）
                    obj.transform.rotation = Quaternion.Inverse(cancelRot) * currentRot;
                }
            }
        }


    }
}

