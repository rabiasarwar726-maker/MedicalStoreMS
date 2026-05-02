// UI/Themes/AppTheme.cs  — Complete design system with hover-ready colors
using System.Drawing;

namespace MedicalStoreMS.UI.Themes
{
    public static class AppTheme
    {
        public static readonly Color Primary       = Color.FromArgb(13,  71, 161);
        public static readonly Color PrimaryLight  = Color.FromArgb(21, 101, 192);
        public static readonly Color PrimaryDark   = Color.FromArgb( 8,  48, 107);
        public static readonly Color Accent        = Color.FromArgb( 0, 188, 140);
        public static readonly Color AccentLight   = Color.FromArgb( 0, 210, 160);
        public static readonly Color AccentDark    = Color.FromArgb( 0, 150, 110);
        public static readonly Color Success       = Color.FromArgb( 46, 160,  67);
        public static readonly Color SuccessLight  = Color.FromArgb( 64, 196,  99);
        public static readonly Color SuccessDark   = Color.FromArgb( 30, 120,  48);
        public static readonly Color Warning       = Color.FromArgb(221, 130,   0);
        public static readonly Color WarningLight  = Color.FromArgb(255, 165,  30);
        public static readonly Color Danger        = Color.FromArgb(198,  40,  40);
        public static readonly Color DangerLight   = Color.FromArgb(229,  57,  53);
        public static readonly Color Info          = Color.FromArgb(  2, 119, 189);
        public static readonly Color InfoLight     = Color.FromArgb(  3, 155, 229);
        public static readonly Color Background    = Color.FromArgb(242, 245, 250);
        public static readonly Color Surface       = Color.White;
        public static readonly Color Border        = Color.FromArgb(213, 220, 232);
        public static readonly Color TextPrimary   = Color.FromArgb( 18,  30,  55);
        public static readonly Color TextSecondary = Color.FromArgb(100, 116, 139);
        public static readonly Color TextMuted     = Color.FromArgb(148, 163, 184);
        public static readonly Color TextLight     = Color.White;
        public static readonly Color Sidebar       = Color.FromArgb( 13,  71, 161);
        public static readonly Color SidebarDark   = Color.FromArgb(  8,  48, 107);
        public static readonly Color SidebarHover  = Color.FromArgb( 25,  95, 190);
        public static readonly Color SidebarActive = Color.FromArgb(  0, 188, 140);
        public static readonly Color SidebarText   = Color.FromArgb(180, 210, 245);
        public static readonly Font FontH1      = new Font("Segoe UI Semibold", 16, FontStyle.Bold);
        public static readonly Font FontH2      = new Font("Segoe UI Semibold", 13, FontStyle.Bold);
        public static readonly Font FontBody    = new Font("Segoe UI", 10, FontStyle.Regular);
        public static readonly Font FontSmall   = new Font("Segoe UI", 9,  FontStyle.Regular);
        public static readonly Font FontButton  = new Font("Segoe UI Semibold", 9,  FontStyle.Bold);
        public static readonly Font FontNav     = new Font("Segoe UI", 10, FontStyle.Regular);
    }
}
