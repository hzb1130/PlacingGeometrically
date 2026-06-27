#nullable disable
using ModSettings;
using UnityEngine;
using System.Reflection;

namespace PlacingGeometrically
{
    internal class PlacingGeometricallySettings : JsonModSettings
    {
        //==================== 总开关 ==中英文改三处注释

        // [Section("总开关")]

        // [Name("启用任意放置")]
        // [Description("开启任意放置")]
        // public bool enableAnyPlace = true;

        // //==================== 第二层：细致摆放 ====================

        // [Section("细致摆放")]

        // [Name("启用细致摆放模式")]
        // [Description("开启后解锁高级摆放（HUD、旋转、微调等）")]
        // public bool enableFineMode = true;

        // //==================== 基础参数 ====================

        // [Section("基础参数")]

        // [Name("细调移动步长")]
        // [Slider(0.01f, 1f, 100, NumberFormat = "{0:F2}")]
        // [Description("每次按键后移动的步长")]
        // public float fineMoveStep = 0.1f;

        // [Name("旋转角大小")]
        
        // [Slider(0, 360)]
        // [Description("每次按键后旋转的角度")]
        // public int rotateDivide = 30;

        // //==================== 辅助显示 ====================

        // [Section("辅助显示")]

        // [Name("开启物品辅助线条")]
        // [Description("选择辅助三轴的方向模式")]
        // [Choice("不显示","世界坐标XYZ","物品局部XYZ")]
        // public int axisDisplayMode = 0;

        // [Name("辅助网格显示模式")]
        // [Description("显示一个网格")]
        // [Choice("关闭", "始终开启", "仅网格模式")]
        // public int gridDisplayMode = 2;

        // //==================== HUD ====================

        // [Section("HUD设置")]

        // [Name("HUD横向偏移(X)")]
        // [Slider(-500, 500)]
        // public int hudOffsetX = -400;

        // [Name("HUD纵向偏移(Y)")]
        // [Slider(-400, 400)]
        // public int hudOffsetY = 200;

        // //==================== 物品信息 ====================

        // [Section("物品信息")]

        // [Name("显示物品信息")]
        // [Description("显示坐标与旋转信息")]
        // public bool enablePlacementInfo = true;

        // [Name("信息X偏移")]
        // [Slider(0, 2000)]
        // [Description("距离屏幕右下角的横向偏移")]
        // public int infoOffsetX = 0;

        // [Name("信息Y偏移")]
        // [Slider(0, 1000)]
        // [Description("距离屏幕右下角的纵向偏移")]
        // public int infoOffsetY = 0;

        // //==================== 模式按键 ====================

        // [Section("模式切换按键")]

        // [Name("网格/吸附 循环切换")]
        // [Description("按顺序切换：普通 → 网格放置 → 物品吸附 → 普通")]
        // public KeyCode keyGridMode = KeyCode.Keypad7;

        // [Name("高度叠叠乐")]
        // [Description("配合吸附，实现堆叠")]
        // public KeyCode keyYCollisionStack = KeyCode.Keypad8;

        // [Name("锁定 坐标/旋转")]
        // [Description("锁定坐标开始旋转物品/锁定旋转移动物品")]
        // public KeyCode keyCoordRotateSwitch = KeyCode.Keypad9;

        // [Name("旋转归零")]
        // [Description("旋转角归零")]
        // public KeyCode keyResetRotateZero = KeyCode.Keypad0;

        // //==================== XYZ微调 ====================

        // [Section("XYZ微调")]

        // [Name("启用XYZ微调")]
        // public bool enableXYZAdjust = false;

        // [Name("X+")]
        // public KeyCode keyXPlus = KeyCode.Keypad1;

        // [Name("X-")]
        // public KeyCode keyXMinus = KeyCode.Keypad4;

        // [Name("Y+")]
        // public KeyCode keyYPlus = KeyCode.Keypad2;

        // [Name("Y-")]
        // public KeyCode keyYMinus = KeyCode.Keypad5;

        // [Name("Z+")]
        // public KeyCode keyZPlus = KeyCode.Keypad3;

        // [Name("Z-")]
        // public KeyCode keyZMinus = KeyCode.Keypad6;
        //==================== Master Toggle ====================

        [Section("Master Toggle")]

        [Name("Enable Any Placement")]
        [Description("Enable unrestricted item placement anywhere")]
        public bool enableAnyPlace = true;

        //==================== Tier 2: Fine Placement ====================

        [Section("Fine Placement")]

        [Name("Enable Fine Placement Mode")]
        [Description("Unlock advanced placement features (HUD, rotation, fine adjustment, stacking)")]
        public bool enableFineMode = true;

        //==================== Basic Settings ====================

        [Section("Basic Settings")]

        [Name("Fine Movement Step")]
        [Slider(0.01f, 1f, 100, NumberFormat = "{0:F2}")]
        [Description("Distance moved per key press")]
        public float fineMoveStep = 0.1f;

        [Name("Rotation Step Angle")]
        [Slider(0, 360)]
        [Description("Rotation angle per key press")]
        public int rotateDivide = 30;

        //==================== Visual Aids ====================

        [Section("Visual Aids")]

        [Name("Show Axis Lines")]
        [Description("Select axis display mode")]
        [Choice("Disabled", "World XYZ", "Local Object XYZ")]
        public int axisDisplayMode = 0;

        [Name("Grid Display Mode")]
        [Description("Show placement grid overlay")]
        [Choice("Disabled", "Always On", "Grid Mode Only")]
        public int gridDisplayMode = 2;

        //==================== HUD ====================

        [Section("HUD Settings")]

        [Name("HUD Offset X")]
        [Slider(-500, 500)]
        public int hudOffsetX = -400;

        [Name("HUD Offset Y")]
        [Slider(-400, 400)]
        public int hudOffsetY = 200;

        //==================== Item Info ====================

        [Section("Item Info")]

        [Name("Show Placement Info")]
        [Description("Display position and rotation info on screen")]
        public bool enablePlacementInfo = true;

        [Name("Info Offset X")]
        [Slider(0, 2000)]
        [Description("Horizontal offset from bottom-right corner")]
        public int infoOffsetX = 0;

        [Name("Info Offset Y")]
        [Slider(0, 1000)]
        [Description("Vertical offset from bottom-right corner")]
        public int infoOffsetY = 0;

        //==================== Mode Hotkeys ====================

        [Section("Mode Switch Keys")]

        [Name("Grid / Snap Cycle Mode")]
        [Description("Cycle: Normal → Grid Snap → Object Snap → Normal")]
        public KeyCode keyGridMode = KeyCode.Keypad7;

        [Name("Vertical Stacking")]
        [Description("Enable vertical stacking on top of other objects")]
        public KeyCode keyYCollisionStack = KeyCode.Keypad8;

        [Name("Transform Lock Cycle")]
        [Description("Cycle: Normal → Lock Position (Rotate Mode) → Lock Rotation (Move Mode) → Normal")]
        public KeyCode keyCoordRotateSwitch = KeyCode.Keypad9;

        [Name("Reset Rotation")]
        [Description("Reset object rotation")]
        public KeyCode keyResetRotateZero = KeyCode.Keypad0;

        //==================== XYZ Fine Adjustment ====================

        [Section("XYZ Adjustment")]

        [Name("Enable XYZ Adjustment")]
        public bool enableXYZAdjust = false;

        [Name("X+")]
        public KeyCode keyXPlus = KeyCode.Keypad1;

        [Name("X-")]
        public KeyCode keyXMinus = KeyCode.Keypad4;

        [Name("Y+")]
        public KeyCode keyYPlus = KeyCode.Keypad2;

        [Name("Y-")]
        public KeyCode keyYMinus = KeyCode.Keypad5;

        [Name("Z+")]
        public KeyCode keyZPlus = KeyCode.Keypad3;

        [Name("Z-")]
        public KeyCode keyZMinus = KeyCode.Keypad6;

        //==================== 动态折叠逻辑 ====================

        protected override void OnChange(FieldInfo field, object oldValue, object newValue)
        {
            base.OnChange(field, oldValue, newValue);

            if (field.Name == nameof(enableAnyPlace) ||
                field.Name == nameof(enableFineMode) ||
                field.Name == nameof(enablePlacementInfo) ||
                field.Name == nameof(enableXYZAdjust))
            {
                RefreshAll();
            }
        }

        public void RefreshAll()
        {
            UpdateAllVisibility(enableAnyPlace);
        }

        public void UpdateAllVisibility(bool isEnable)
        {
            //==================== 第一层：总开关 ====================

            if (!isEnable)
            {
                SetAllFieldsInvisible();
                return;
            }

            // 总开关下，只显示细致摆放
            SetFieldVisible(nameof(enableFineMode), true);

            bool fine = enableFineMode;

            //==================== 第二层：细致摆放 ====================

            // 基础参数
            SetFieldVisible(nameof(fineMoveStep), fine);
            SetFieldVisible(nameof(rotateDivide), fine);

            // 辅助显示
            SetFieldVisible(nameof(axisDisplayMode), fine);
            SetFieldVisible(nameof(gridDisplayMode), fine);

            // HUD
            SetFieldVisible(nameof(hudOffsetX), fine);
            SetFieldVisible(nameof(hudOffsetY), fine);

            // 物品信息
            SetFieldVisible(nameof(enablePlacementInfo), fine);

            bool infoVisible = fine && enablePlacementInfo;

            SetFieldVisible(nameof(infoOffsetX), infoVisible);
            SetFieldVisible(nameof(infoOffsetY), infoVisible);

            // 模式按键
            SetFieldVisible(nameof(keyGridMode), fine);
            // SetFieldVisible(nameof(keySnapItemXZ), fine);
            SetFieldVisible(nameof(keyYCollisionStack), fine);
            SetFieldVisible(nameof(keyCoordRotateSwitch), fine);
            SetFieldVisible(nameof(keyResetRotateZero), fine);

            // XYZ微调
            SetFieldVisible(nameof(enableXYZAdjust), fine);

            bool xyz = fine && enableXYZAdjust;

            // XYZ按键
            SetFieldVisible(nameof(keyXPlus), xyz);
            SetFieldVisible(nameof(keyXMinus), xyz);
            SetFieldVisible(nameof(keyYPlus), xyz);
            SetFieldVisible(nameof(keyYMinus), xyz);
            SetFieldVisible(nameof(keyZPlus), xyz);
            SetFieldVisible(nameof(keyZMinus), xyz);
        }

        private void SetAllFieldsInvisible()
        {
            SetFieldVisible(nameof(enableFineMode), false);

            // 基础参数
            SetFieldVisible(nameof(fineMoveStep), false);
            SetFieldVisible(nameof(rotateDivide), false);

            // 辅助显示
            SetFieldVisible(nameof(axisDisplayMode), false);
            SetFieldVisible(nameof(gridDisplayMode), false);

            // HUD
            SetFieldVisible(nameof(hudOffsetX), false);
            SetFieldVisible(nameof(hudOffsetY), false);

            // 物品信息
            SetFieldVisible(nameof(enablePlacementInfo), false);
            SetFieldVisible(nameof(infoOffsetX), false);
            SetFieldVisible(nameof(infoOffsetY), false);

            // 模式按键
            SetFieldVisible(nameof(keyGridMode), false);
            // SetFieldVisible(nameof(keySnapItemXZ), false);
            SetFieldVisible(nameof(keyYCollisionStack), false);
            SetFieldVisible(nameof(keyCoordRotateSwitch), false);
            SetFieldVisible(nameof(keyResetRotateZero), false);

            // XYZ
            SetFieldVisible(nameof(enableXYZAdjust), false);

            SetFieldVisible(nameof(keyXPlus), false);
            SetFieldVisible(nameof(keyXMinus), false);
            SetFieldVisible(nameof(keyYPlus), false);
            SetFieldVisible(nameof(keyYMinus), false);
            SetFieldVisible(nameof(keyZPlus), false);
            SetFieldVisible(nameof(keyZMinus), false);
        }

        protected override void OnConfirm()
        {
            base.OnConfirm();
            RefreshAll();
        }
    }

    internal static class Settings
    {
        public static PlacingGeometricallySettings options;

        public static void OnLoad()
        {
            options = new PlacingGeometricallySettings();

            // options.AddToModSettings("几何学放置 v1.2");
            options.AddToModSettings("Placing Geometrically v1.2");

            options.RefreshAll();
        }
    }
}