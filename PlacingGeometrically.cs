using MelonLoader;
using HarmonyLib;
using UnityEngine;
using Il2Cpp;
using System.Collections.Generic;
using Il2CppTLD.Placement;

[assembly: MelonInfo(typeof(PlacingGeometrically.PlacingGeometricallyMain), "Placing Geometrically", "1.2.0", "hzb1130")]
[assembly: MelonGame("Hinterland", "TheLongDark")]

namespace PlacingGeometrically
{
    public class PlacingGeometricallyMain : MelonMod
    {
        private GameObject? paHUD = null;
        private readonly Dictionary<KeyCode, ButtonPrompt> buttonMap = new Dictionary<KeyCode, ButtonPrompt>();

        private bool gridModeOn = false;
        private bool snapXZOn = false;
        private bool yCollisionOn = false;
        private bool coordRotateSwitchOn = false;
        private bool coordPositionSwitchOn = false;
        private Vector3 _extraOffset = Vector3.zero;
        private Vector3 _extraRotation = Vector3.zero;
        private Vector3 _lockedPosition = Vector3.zero;
        private Vector3 _rotationCancel = Vector3.zero;
        private Quaternion _lockedRotation = Quaternion.identity;
        private PlayerManager? playerManager = null;
        private LineRenderer? _lineX;
        private LineRenderer? _lineY;
        private LineRenderer? _lineZ;
        private GUIStyle? _placementInfoStyle;
        private string _placementInfo = "";
        private readonly List<LineRenderer> _gridLines = new();

        public override void OnInitializeMelon()
        {
            Settings.OnLoad();
        }

        public override void OnGUI()
        {
            base.OnGUI();
            DrawPlacementInfo();
            
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            UpdatePlacementAxisLine();

            UpdateGridPlane();

            if (!Settings.options.enableFineMode && paHUD != null)
            {
                DestroyPlacingHUD();
                return;
            }
            if (paHUD == null || !Settings.options.enableFineMode) return;

            // ==========单按键循环切换==========
            if (Input.GetKeyDown(Settings.options.keyGridMode))
            {
                if (!gridModeOn && !snapXZOn)
                {
                    // 状态：普通 → 网格
                    gridModeOn = true;
                    snapXZOn = false;
                }
                else if (gridModeOn)
                {
                    // 状态：网格 → 吸附
                    gridModeOn = false;
                    snapXZOn = true;
                }
                else if (snapXZOn)
                {
                    // 状态：吸附 → 普通
                    gridModeOn = false;
                    snapXZOn = false;
                }
            }

            // 其余功能按键逻辑不变
            if (Input.GetKeyDown(Settings.options.keyYCollisionStack))
                yCollisionOn = !yCollisionOn;
            if (Input.GetKeyDown(Settings.options.keyCoordRotateSwitch))
            {
                GameObject? obj = playerManager?.GetObjectToPlace();

                // 普通 -> 旋转
                if (!coordRotateSwitchOn && !coordPositionSwitchOn)
                {
                    coordRotateSwitchOn = true;

                    if (obj != null)
                    {
                        _lockedPosition = obj.transform.position;
                        _extraRotation = obj.transform.eulerAngles;
                        _rotationCancel = Vector3.zero;
                    }
                }
                // 旋转 -> 位置
                else if (coordRotateSwitchOn)
                {
                    coordRotateSwitchOn = false;
                    coordPositionSwitchOn = true;

                    if (obj != null)
                    {
                        _lockedRotation = obj.transform.rotation;
                        _lockedPosition = obj.transform.position;
                    }
                }
                // 位置 -> 普通
                else
                {
                    coordPositionSwitchOn = false;
                }
            }

            var keys = new List<KeyCode>(buttonMap.Keys);
            foreach (var key in keys)
            {
                if (!buttonMap.TryGetValue(key, out var button)) continue;
                Color targetColor = Color.white;

                bool isModeKey = key == Settings.options.keyGridMode ||
                                key == Settings.options.keyYCollisionStack ||
                                key == Settings.options.keyCoordRotateSwitch;

                if (isModeKey)
                {
                    if (key == Settings.options.keyGridMode)
                    {
                        if (gridModeOn)
                            targetColor = new Color32(255, 140, 0, 255);      // 橙：网格
                        else if (snapXZOn)
                            targetColor = new Color32(0, 220, 220, 255);      // 青：吸附
                    }
                    else if (key == Settings.options.keyYCollisionStack)
                    {
                        if (yCollisionOn)
                            targetColor = new Color32(255, 140, 0, 255);
                    }
                    else if (key == Settings.options.keyCoordRotateSwitch)
                    {
                        if (coordRotateSwitchOn)
                            targetColor = new Color32(255, 140, 0, 255);      // 橙：旋转模式
                        else if (coordPositionSwitchOn)
                            targetColor = new Color32(0, 220, 220, 255);      // 青：位置模式
                    }
                }
                else
                {
                    targetColor = Input.GetKey(key)
                        ? new Color32(255, 140, 0, 255)
                        : Color.white;
                }

                button.m_KeyboardButtonLabel.color = targetColor;
                button.m_KeyboardButtonSprite.color = targetColor;
            }
        }

        private void DrawPlacementInfo()
        {
            if (!Settings.options.enableAnyPlace ||
                !Settings.options.enableFineMode ||
                !Settings.options.enablePlacementInfo ||
                playerManager == null ||
                !playerManager.IsInMeshPlacementMode())
                return;

            GameObject obj = playerManager.GetObjectToPlace();

            if (obj == null)
                return;

            // 只初始化一次
            if (_placementInfoStyle == null)
            {
                _placementInfoStyle = new GUIStyle(GUI.skin.box)
                {
                    fontSize = 18,
                    font = Font.CreateDynamicFontFromOSFont("Consolas", 18),
                    alignment = TextAnchor.MiddleLeft,
                    wordWrap = false,
                    padding = new RectOffset(8, 8, 4, 4)
                };
            }

            Vector3 pos = obj.transform.position;
            Vector3 rot = obj.transform.eulerAngles;

            rot.x = NormalizeAngle(rot.x);
            rot.y = NormalizeAngle(rot.y);
            rot.z = NormalizeAngle(rot.z);

            // _placementInfo =
            //     $"位置 X:{FormatSigned(pos.x)}  Y:{FormatSigned(pos.y)}  Z:{FormatSigned(pos.z)}\n" +
            //     $"旋转 X:{FormatSigned(rot.x)}  Y:{FormatSigned(rot.y)}  Z:{FormatSigned(rot.z)}";
            //ENG
            _placementInfo =
                $"Position X:{FormatSigned(pos.x)}  Y:{FormatSigned(pos.y)}  Z:{FormatSigned(pos.z)}\n" +
                $"Rotation X:{FormatSigned(rot.x)}  Y:{FormatSigned(rot.y)}  Z:{FormatSigned(rot.z)}";


            Vector2 size = _placementInfoStyle.CalcSize(new GUIContent(_placementInfo));

            float width = size.x + 16f;
            float height = size.y + 10f;

            Rect rect = new Rect(
                Screen.width - width - Settings.options.infoOffsetX,
                Screen.height - height - Settings.options.infoOffsetY,
                width,
                height
            );

            GUI.Box(rect, _placementInfo, _placementInfoStyle);
        }

        private string FormatSigned(float value)
        {
            char sign = value >= 0 ? '+' : '-';

            value = Mathf.Abs(value);

            int i = Mathf.FloorToInt(value);

            int d = Mathf.RoundToInt((value - i) * 100f);

            if (d >= 100)
            {
                i++;
                d = 0;
            }

            return $"{sign}{i.ToString().PadLeft(4, ' ')}.{d:00}";
        }

        private float NormalizeAngle(float angle)
        {
            if (angle > 180f)
                angle -= 360f;

            return angle;
        }

        private void UpdatePlacementAxisLine()
        {
            if(!Settings.options.enableAnyPlace || !Settings.options.enableFineMode)
                return;
            if (playerManager == null) return;
            if (!playerManager.IsInMeshPlacementMode()) return;

            GameObject obj = playerManager.GetObjectToPlace();

            // 没有物体时隐藏全部轴
            if (obj == null)
            {
                if (_lineX != null)
                    _lineX.gameObject.SetActive(false);

                if (_lineY != null)
                    _lineY.gameObject.SetActive(false);

                if (_lineZ != null)
                    _lineZ.gameObject.SetActive(false);

                return;
            }

            // 0 = 不显示
            if (Settings.options.axisDisplayMode == 0)
            {
                if (_lineX != null)
                    _lineX.gameObject.SetActive(false);

                if (_lineY != null)
                    _lineY.gameObject.SetActive(false);

                if (_lineZ != null)
                    _lineZ.gameObject.SetActive(false);

                return;
            }

            Vector3 origin = obj.transform.position;

            // =========================
            // 创建 X 轴
            // =========================

            if (_lineX == null)
            {
                GameObject go = new GameObject("AxisX");

                _lineX = go.AddComponent<LineRenderer>();

                _lineX.positionCount = 2;
                _lineX.startWidth = 0.03f;
                _lineX.endWidth = 0.03f;

                Shader shader = Shader.Find("Sprites/Default");

                _lineX.material = new Material(shader);

                _lineX.material.color = Color.red;

                _lineX.useWorldSpace = true;

                _lineX.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                _lineX.receiveShadows = false;
            }

            // =========================
            // 创建 Y 轴
            // =========================

            if (_lineY == null)
            {
                GameObject go = new GameObject("AxisY");

                _lineY = go.AddComponent<LineRenderer>();

                _lineY.positionCount = 2;
                _lineY.startWidth = 0.03f;
                _lineY.endWidth = 0.03f;

                Shader shader = Shader.Find("Sprites/Default");

                _lineY.material = new Material(shader);

                _lineY.material.color = Color.green;

                _lineY.useWorldSpace = true;

                _lineY.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                _lineY.receiveShadows = false;
            }

            // =========================
            // 创建 Z 轴
            // =========================

            if (_lineZ == null)
            {
                GameObject go = new GameObject("AxisZ");

                _lineZ = go.AddComponent<LineRenderer>();

                _lineZ.positionCount = 2;
                _lineZ.startWidth = 0.03f;
                _lineZ.endWidth = 0.03f;

                Shader shader = Shader.Find("Sprites/Default");

                _lineZ.material = new Material(shader);

                _lineZ.material.color = Color.blue;

                _lineZ.useWorldSpace = true;

                _lineZ.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                _lineZ.receiveShadows = false;
            }

            // =========================
            // 显示轴
            // =========================

            _lineX.gameObject.SetActive(true);
            _lineY.gameObject.SetActive(true);
            _lineZ.gameObject.SetActive(true);

            // =========================
            // 根据模式决定方向
            // =========================

            Vector3 dirX;
            Vector3 dirY;
            Vector3 dirZ;

            // 1 = 世界坐标XYZ
            if (Settings.options.axisDisplayMode == 1)
            {
                dirX = Vector3.right;
                dirY = Vector3.up;
                dirZ = Vector3.forward;
            }
            // 2 = 物品局部XYZ
            else
            {
                dirX = obj.transform.right;
                dirY = obj.transform.up;
                dirZ = obj.transform.forward;
            }

            // =========================
            // 更新轴位置
            // =========================

            float axisLength = 0.5f;

            _lineX.SetPosition(0, origin);
            _lineX.SetPosition(1, origin + dirX * axisLength);

            _lineY.SetPosition(0, origin);
            _lineY.SetPosition(1, origin + dirY * axisLength);

            _lineZ.SetPosition(0, origin);
            _lineZ.SetPosition(1, origin + dirZ * axisLength);
        }

        private void UpdateGridPlane()
        {
            if(!Settings.options.enableAnyPlace || !Settings.options.enableFineMode)
                return;
            if (playerManager == null) return;

            GameObject obj = playerManager.GetObjectToPlace();

            // 没有物体或关闭模式时隐藏网格
            if (obj == null || Settings.options.gridDisplayMode == 0)
            {
                foreach (var line in _gridLines)
                {
                    if (line == null)
                        continue;

                    if (line.gameObject != null)
                        line.gameObject.SetActive(false);
                }
                return;
            }

            // 如果网格线数量不足，创建6条
            while (_gridLines.Count < 6)
            {
                GameObject go = new GameObject("GridLine");
                LineRenderer line = go.AddComponent<LineRenderer>();

                line.positionCount = 2;
                line.startWidth = 0.02f;
                line.endWidth = 0.02f;

                Shader shader = Shader.Find("Sprites/Default");
                line.material = new Material(shader);
                line.material.color = new Color(1f, 1f, 1f, 0.7f);

                line.useWorldSpace = true;
                line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                line.receiveShadows = false;

                _gridLines.Add(line);
            }

            // =========================
            // 决定是否显示网格
            // =========================

            // 1 = 始终开启
            // 2 = 仅网格模式开启（仅在物品网格模式下显示）
            bool showGrid = Settings.options.gridDisplayMode == 1 || (Settings.options.gridDisplayMode == 2 && gridModeOn);

            foreach (var line in _gridLines)
            {
                if (line == null)
                    continue;

                if (line.gameObject != null)
                    line.gameObject.SetActive(showGrid);
            }
            if (!showGrid) return;

            // =========================
            // 计算网格中心和偏移
            // =========================
            Vector3 pos = obj.transform.position;

            float x = Mathf.Round(pos.x);
            float z = Mathf.Round(pos.z);

            Vector3 center = new Vector3(x, pos.y, z);

            float half = 1f;

            // 横线
            for (int i = 0; i < 3; i++)
            {
                float offset = -1f + i;
                LineRenderer line = _gridLines[i];
                if (line == null)
                    continue;

                line.SetPosition(0, center + new Vector3(-half, 0, offset));
                line.SetPosition(1, center + new Vector3( half, 0, offset));
            }

            // 竖线
            for (int i = 0; i < 3; i++)
            {
                float offset = -1f + i;
                LineRenderer line = _gridLines[i + 3];

                line.SetPosition(0, center + new Vector3(offset, 0, -half));
                line.SetPosition(1, center + new Vector3(offset, 0,  half));
            }
        }

        private void CreatePlacingHUD()
        {
            if (paHUD != null) return;
            if (!Settings.options.enableFineMode)
            {
                gridModeOn = false; snapXZOn = false; yCollisionOn = false; coordRotateSwitchOn = false;coordPositionSwitchOn = false;
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
                go.transform.localScale = Vector3.one;
                go.transform.localRotation = Quaternion.identity;
                go.transform.localPosition = Vector3.zero;
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
                // float posX = baseX + colIndex * colWidth;
                // float posY = baseY - rowIndex * rowHeight;
                float posX =
                    Settings.options.hudOffsetX +
                    colIndex * colWidth;

                float posY =
                    Settings.options.hudOffsetY -
                    rowIndex * rowHeight;
                go.transform.localPosition = new Vector3(posX, posY, 0);
                if (!buttonMap.ContainsKey(keyCode)) buttonMap.Add(keyCode, button);
            }

            // AddButton("网格/吸附", Settings.options.keyGridMode, 0, 0);
            // AddButton("高度叠放", Settings.options.keyYCollisionStack, 0, 1);
            // AddButton("锁定 坐标/旋转", Settings.options.keyCoordRotateSwitch, 1, 0);
            // AddButton("归零旋转", Settings.options.keyResetRotateZero, 1, 1);
            //ENG
            AddButton("Grid / Snap", Settings.options.keyGridMode, 0, 0);
            AddButton("Vertical Stack", Settings.options.keyYCollisionStack, 0, 1);
            AddButton("Lock Pos / Rot", Settings.options.keyCoordRotateSwitch, 1, 0);
            AddButton("Reset Rotation", Settings.options.keyResetRotateZero, 1, 1);

            if (Settings.options.enableXYZAdjust)
            {
                AddButton("X+", Settings.options.keyXPlus, 2, 0);
                AddButton("X-", Settings.options.keyXMinus, 2, 1);
                AddButton("Y+", Settings.options.keyYPlus, 3, 0);
                AddButton("Y-", Settings.options.keyYMinus, 3, 1);
                AddButton("Z+", Settings.options.keyZPlus, 4, 0);
                AddButton("Z-", Settings.options.keyZMinus, 4, 1);
            }
        }

        private void DestroyPlacingHUD()
        {
            if (paHUD != null) { UnityEngine.Object.Destroy(paHUD); paHUD = null; buttonMap.Clear(); }
            _extraOffset = Vector3.zero; _extraRotation = Vector3.zero; _lockedPosition = Vector3.zero; _rotationCancel = Vector3.zero;
            playerManager = null; coordRotateSwitchOn = false;coordPositionSwitchOn = false;_lockedRotation = Quaternion.identity;
            // 隐藏或销毁所有三轴
            if (_lineX != null) _lineX.gameObject.SetActive(false);
            if (_lineY != null) _lineY.gameObject.SetActive(false);
            if (_lineZ != null) _lineZ.gameObject.SetActive(false);

            // 销毁 LineRenderer 对象
            if (_lineX != null)
            {
                UnityEngine.Object.Destroy(_lineX.gameObject);
                _lineX = null;
            }

            if (_lineY != null)
            {
                UnityEngine.Object.Destroy(_lineY.gameObject);
                _lineY = null;
            }

            if (_lineZ != null)
            {
                UnityEngine.Object.Destroy(_lineZ.gameObject);
                _lineZ = null;
            }

            // 隐藏网格线
            foreach (var line in _gridLines)
            {
                if (line == null)
                    continue;

                if (line.gameObject != null)
                    line.gameObject.SetActive(false);
            }
            _gridLines.Clear();

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
                if (d < bestDist) { bestDist = d; best = gear; }
            }
            return best;
        }

        [HarmonyPatch(typeof(PlayerManager), "StartPlaceMesh", new[] { typeof(GameObject), typeof(float), typeof(PlaceMeshFlags), typeof(PlaceMeshRules) })]
        internal static class Patch_StartPlaceMesh
        {
            private static void Postfix(PlayerManager __instance)
            {
                var mod = Melon<PlacingGeometricallyMain>.Instance;
                mod.playerManager = __instance;
                mod.CreatePlacingHUD();
                GameObject obj = __instance.GetObjectToPlace();
                if (obj != null) mod._extraRotation = obj.transform.eulerAngles;
                if (mod.paHUD != null && !Utils.IsGamepadActive()) mod.paHUD.SetActive(true);
            }
        }

        [HarmonyPatch(typeof(PlayerManager), "ExitMeshPlacement")]
        internal static class Patch_ExitMeshPlacement
        {
            private static void Postfix() { Melon<PlacingGeometricallyMain>.Instance.DestroyPlacingHUD(); }
        }

        [HarmonyPatch(typeof(PlayerManager), "CancelPlaceMesh")]
        internal static class Patch_CancelPlaceMesh
        {
            private static void Postfix() { Melon<PlacingGeometricallyMain>.Instance.DestroyPlacingHUD(); }
        }

        [HarmonyPatch(typeof(PlayerManager), nameof(PlayerManager.DoPositionCheck))]
        internal static class Patch_DoPositionCheck
        {
            private static void Postfix(PlayerManager __instance, ref MeshLocationCategory __result)
            {
                if (!Settings.options.enableAnyPlace) return;

                __result = MeshLocationCategory.Valid;

                if (!Settings.options.enableFineMode) return;

                var mod = Melon<PlacingGeometricallyMain>.Instance;
                GameObject obj = __instance.GetObjectToPlace();

                if (obj == null) return;

                float rotateStep = Settings.options.rotateDivide;
                float moveStep = Settings.options.fineMoveStep;
                bool xyzEnabled = Settings.options.enableXYZAdjust;
                bool resetPressed = Input.GetKeyDown(Settings.options.keyResetRotateZero);

                // ==================================================
                // 坐标旋转模式：独立流程
                // ==================================================

                if (mod.coordRotateSwitchOn)
                {
                    if (resetPressed) {mod._rotationCancel = mod._extraRotation; }

                    if (xyzEnabled)
                    {
                        if (Input.GetKeyDown(Settings.options.keyXPlus)) mod._extraRotation.x += rotateStep;
                        if (Input.GetKeyDown(Settings.options.keyXMinus)) mod._extraRotation.x -= rotateStep;
                        if (Input.GetKeyDown(Settings.options.keyYPlus)) mod._extraRotation.y += rotateStep;
                        if (Input.GetKeyDown(Settings.options.keyYMinus)) mod._extraRotation.y -= rotateStep;
                        if (Input.GetKeyDown(Settings.options.keyZPlus)) mod._extraRotation.z += rotateStep;
                        if (Input.GetKeyDown(Settings.options.keyZMinus)) mod._extraRotation.z -= rotateStep;
                    }

                    Quaternion coordRotation = Quaternion.Euler(mod._extraRotation);

                    if (mod._rotationCancel != Vector3.zero)
                    {
                        Quaternion cancelRot = Quaternion.Euler(mod._rotationCancel);
                        coordRotation = Quaternion.Inverse(cancelRot) * coordRotation;
                    }

                    obj.transform.position = mod._lockedPosition;
                    obj.transform.rotation = coordRotation;
                    return;
                }

                if (mod.coordPositionSwitchOn)
                {
                    if (resetPressed)
                    {
                        mod._lockedPosition = obj.transform.position;
                    }

                    if (xyzEnabled)
                    {
                        if (Input.GetKeyDown(Settings.options.keyXPlus))
                            mod._lockedPosition.x += moveStep;

                        if (Input.GetKeyDown(Settings.options.keyXMinus))
                            mod._lockedPosition.x -= moveStep;

                        if (Input.GetKeyDown(Settings.options.keyYPlus))
                            mod._lockedPosition.y += moveStep;

                        if (Input.GetKeyDown(Settings.options.keyYMinus))
                            mod._lockedPosition.y -= moveStep;

                        if (Input.GetKeyDown(Settings.options.keyZPlus))
                            mod._lockedPosition.z += moveStep;

                        if (Input.GetKeyDown(Settings.options.keyZMinus))
                            mod._lockedPosition.z -= moveStep;
                    }

                    obj.transform.position = mod._lockedPosition;
                    obj.transform.rotation = mod._lockedRotation;

                    return;
                }

                // ==================================================
                // 根据当前模式预扫描 Gear
                // ==================================================

                GearItem? snapGear = null;
                GearItem? stackRayGear = null;

                if (mod.snapXZOn)
                {
                    int snapLayerMask = PlayerManager.GetLayerMaskForPlaceMeshRaycast();
                    snapLayerMask |= 1 << vp_Layer.Gear;
                    snapGear = FindGearByCameraOrNearby(obj.transform.position, snapLayerMask);
                }

                if (mod.yCollisionOn)
                {
                    int stackLayerMask = 1 << vp_Layer.Gear;
                    stackRayGear = FindGearByCamera(stackLayerMask);
                }

                // ==================================================
                // Reset
                // ==================================================

                if (resetPressed)
                {
                    if (mod.snapXZOn)
                    {
                        if (snapGear != null)
                        {
                            mod._rotationCancel = snapGear.transform.eulerAngles;
                            obj.transform.rotation = snapGear.transform.rotation;
                        }
                    }
                    else
                    {
                        mod._rotationCancel = obj.transform.eulerAngles;
                    }
                }

                // ==================================================
                // 移动输入
                // ==================================================

                if (xyzEnabled)
                {
                    if (Input.GetKeyDown(Settings.options.keyXPlus)) mod._extraOffset.x += moveStep;
                    if (Input.GetKeyDown(Settings.options.keyXMinus)) mod._extraOffset.x -= moveStep;
                    if (Input.GetKeyDown(Settings.options.keyYPlus)) mod._extraOffset.y += moveStep;
                    if (Input.GetKeyDown(Settings.options.keyYMinus)) mod._extraOffset.y -= moveStep;
                    if (Input.GetKeyDown(Settings.options.keyZPlus)) mod._extraOffset.z += moveStep;
                    if (Input.GetKeyDown(Settings.options.keyZMinus)) mod._extraOffset.z -= moveStep;
                }

                // ==================================================
                // 位置 / 旋转求解
                // ==================================================

                Vector3 targetPosition = obj.transform.position;
                Quaternion targetRotation = obj.transform.rotation;

                if (mod.gridModeOn)
                {
                    targetPosition.x = Mathf.Round(targetPosition.x) + mod._extraOffset.x;
                    targetPosition.z = Mathf.Round(targetPosition.z) + mod._extraOffset.z;
                    targetPosition.y += mod._extraOffset.y;
                }
                else if (mod.snapXZOn)
                {
                    if (snapGear != null)
                    {
                        targetPosition = snapGear.transform.position + mod._extraOffset;
                        targetRotation = snapGear.transform.rotation;
                    }
                    else
                    {
                        // 吸附目标不存在时，退化为普通模式
                        targetPosition += mod._extraOffset;
                    }
                }
                else
                {
                    targetPosition += mod._extraOffset;
                }

                // ==================================================
                // 高度叠放后处理
                // ==================================================

                if (mod.yCollisionOn)
                {
                    GearItem? stackGear = stackRayGear;

                    if (stackGear == null)
                        stackGear = FindNearbyGear(targetPosition);

                    if (stackGear != null && stackGear.gameObject != obj)
                    {
                        bool found = false;

                        // ==================================================
                        // 1. 优先使用 PlacementHelper
                        // ==================================================

                        Collider[] colliders = stackGear.GetComponentsInChildren<Collider>(true);

                        foreach (Collider col in colliders)
                        {
                            if (col == null)
                                continue;

                            if (!col.enabled)
                                continue;

                            if (col.name == "PlacementHelper")
                            {
                                targetPosition.y = col.bounds.max.y + mod._extraOffset.y;
                                found = true;
                                break;
                            }
                        }

                        // ==================================================
                        // 2. 没有 PlacementHelper，则使用第一个 Renderer
                        // ==================================================

                        if (!found)
                        {
                            Renderer renderer = stackGear.GetComponentInChildren<Renderer>();

                            if (renderer != null && renderer.enabled)
                            {
                                targetPosition.y = renderer.bounds.max.y + mod._extraOffset.y;
                            }
                        }
                    }
                }

                // ==================================================
                // 旋转抵消
                // ==================================================

                if (mod._rotationCancel != Vector3.zero)
                {
                    Quaternion cancelRot = Quaternion.Euler(mod._rotationCancel);
                    targetRotation = Quaternion.Inverse(cancelRot) * targetRotation;
                }

                // ==================================================
                // Apply
                // ==================================================

                obj.transform.position = targetPosition;
                obj.transform.rotation = targetRotation;
            }

            private static GearItem? FindGearByCamera(int layerMask, float distance = 6f)
            {
                vp_FPSCamera cam = GameManager.GetVpFPSCamera();

                if (cam == null) return null;

                Ray ray = new Ray(cam.transform.position, cam.transform.forward);
                RaycastHit hit;

                if (!Physics.Raycast(ray, out hit, distance, layerMask)) return null;

                return hit.collider.GetComponentInParent<GearItem>();
            }

            private static GearItem? FindGearByCameraOrNearby(Vector3 nearbyPosition, int layerMask, float distance = 6f, float nearbyRadius = 0.6f)
            {
                GearItem? gear = FindGearByCamera(layerMask, distance);

                if (gear != null) return gear;

                return FindNearbyGear(nearbyPosition, nearbyRadius);
            }
        }

    }
}