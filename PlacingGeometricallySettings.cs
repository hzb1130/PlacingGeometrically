#nullable disable
using ModSettings;
using UnityEngine;
using System.Reflection;

namespace PlacingGeometrically
{
    internal class PlacingGeometricallySettings : JsonModSettings
    {
        //==================== Master Switch ====================
        [Section("Master Switch")]

        [Name("Enable Free Placement")]
        [Description("Allow objects to be placed anywhere without grid or collision restrictions")]
        public bool enableAnyPlace = true;

        //==================== Layer 2: Fine Placement ====================
        [Section("Fine Placement")]

        [Name("Enable Fine Placement Mode")]
        [Description("Enable advanced placement features such as HUD display, rotation control, and precise adjustments")]
        public bool enableFineMode = false;

        //==================== Basic Parameters ====================
        [Section("Basic Parameters")]

        [Name("Fine Movement Step")]
        [Description("Step size used for each fine movement adjustment; smaller values allow more precise control")]
        [Slider(0.1f, 1.0f, 10)]
        public float fineMoveStep = 0.2f;

        [Name("Rotation Division Count")]
        [Description("Number of equal segments for rotation; higher values result in finer rotation increments")]
        [Slider(1, 12, 1)]
        public int rotateDivide = 4;

        //==================== HUD ====================
        [Section("HUD Settings")]

        [Name("HUD Horizontal Offset (X)")]
        [Description("Adjust the horizontal position of the HUD on screen")]
        [Slider(-2000, 2000)]
        public int hudOffsetX = 0;

        [Name("HUD Vertical Offset (Y)")]
        [Description("Adjust the vertical position of the HUD on screen")]
        [Slider(-600, 600)]
        public int hudOffsetY = 0;

        //==================== Mode Keys ====================
        [Section("Mode Toggle Keys")]

        [Name("Grid Placement")]
        [Description("Toggle grid-based placement mode for snapping objects to a grid")]
        public KeyCode keyGridMode = KeyCode.Keypad7;

        [Name("Snap to Item (XZ)")]
        [Description("Snap the current object to the surface of another object along the XZ plane")]
        public KeyCode keySnapItemXZ = KeyCode.Keypad8;

        [Name("Vertical Stack (Y Collision)")]
        [Description("Enable stacking objects vertically based on collision along the Y-axis")]
        public KeyCode keyYCollisionStack = KeyCode.Keypad9;

        [Name("Coordinate/Rotation Toggle")]
        [Description("Switch between coordinate adjustment mode and rotation adjustment mode")]
        public KeyCode keyCoordRotateSwitch = KeyCode.KeypadDivide;

        [Name("Reset Rotation")]
        [Description("Reset the object's rotation back to zero")]
        public KeyCode keyResetRotateZero = KeyCode.Keypad0;

        //==================== XYZ Fine Adjustment ====================
        [Section("XYZ Fine Adjustment")]

        [Name("Enable XYZ Adjustment")]
        [Description("Allow manual fine-tuning of object position along X, Y, and Z axes using hotkeys")]
        public bool enableXYZAdjust = false;

        [Name("X+")]
        [Description("Move the object slightly in the positive X direction")]
        public KeyCode keyXPlus = KeyCode.Keypad1;

        [Name("X-")]
        [Description("Move the object slightly in the negative X direction")]
        public KeyCode keyXMinus = KeyCode.Keypad2;

        [Name("Y+")]
        [Description("Move the object slightly upward along the Y axis")]
        public KeyCode keyYPlus = KeyCode.Keypad3;

        [Name("Y-")]
        [Description("Move the object slightly downward along the Y axis")]
        public KeyCode keyYMinus = KeyCode.Keypad4;

        [Name("Z+")]
        [Description("Move the object slightly forward along the Z axis")]
        public KeyCode keyZPlus = KeyCode.Keypad5;

        [Name("Z-")]
        [Description("Move the object slightly backward along the Z axis")]
        public KeyCode keyZMinus = KeyCode.Keypad6;

        //==================== 动态折叠逻辑 ====================

        protected override void OnChange(FieldInfo field, object oldValue, object newValue)
        {
            base.OnChange(field, oldValue, newValue);

            if (field.Name == nameof(enableAnyPlace) ||
                field.Name == nameof(enableFineMode) ||
                field.Name == nameof(enableXYZAdjust))
            {
                UpdateAllVisibility(enableAnyPlace);
            }
        }

        public void UpdateAllVisibility(bool isEnable)
        {
            // ===== 第一层：总开关 =====
            if (!isEnable)
            {
                SetAllFieldsInvisible();
                return;
            }

            // 总开关下，显示细致摆放开关
            SetFieldVisible(nameof(enableFineMode), true);

            bool fine = enableFineMode;

            // ===== 第二层：细致摆放 =====
            SetFieldVisible(nameof(fineMoveStep), fine);
            SetFieldVisible(nameof(rotateDivide), fine);

            SetFieldVisible(nameof(hudOffsetX), fine);
            SetFieldVisible(nameof(hudOffsetY), fine);

            SetFieldVisible(nameof(keyGridMode), fine);
            SetFieldVisible(nameof(keySnapItemXZ), fine);
            SetFieldVisible(nameof(keyYCollisionStack), fine);
            SetFieldVisible(nameof(keyCoordRotateSwitch), fine);
            SetFieldVisible(nameof(keyResetRotateZero), fine);

            // ===== 第三层：XYZ开关 =====
            SetFieldVisible(nameof(enableXYZAdjust), fine);

            bool xyz = fine && enableXYZAdjust;

            // ===== 第四层：XYZ按键 =====
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

            SetFieldVisible(nameof(fineMoveStep), false);
            SetFieldVisible(nameof(rotateDivide), false);

            SetFieldVisible(nameof(hudOffsetX), false);
            SetFieldVisible(nameof(hudOffsetY), false);

            SetFieldVisible(nameof(keyGridMode), false);
            SetFieldVisible(nameof(keySnapItemXZ), false);
            SetFieldVisible(nameof(keyYCollisionStack), false);
            SetFieldVisible(nameof(keyCoordRotateSwitch), false);
            SetFieldVisible(nameof(keyResetRotateZero), false);

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
            UpdateAllVisibility(enableAnyPlace);
        }
    }

    internal static class Settings
    {
        public static PlacingGeometricallySettings options;

        public static void OnLoad()
        {
            options = new PlacingGeometricallySettings();
            // options.AddToModSettings("几何学放置");
            options.AddToModSettings("Placing Geometrically");
            options.UpdateAllVisibility(options.enableAnyPlace);
        }
    }
}