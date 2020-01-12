using System.Collections.Generic;
using System.Reflection;
using System.Windows.Interop;
using System.Windows.Media;

namespace System.Windows.Extensions
{
    /// <summary>
    /// This class allows changing actual DPI scale for an entire application or selected windows.
    /// </summary>
    public static class DpiHelper
    {
        private static bool _isInitialized;
        private static double _dpiScaleX;
        private static double _dpiScaleY;

        private static MethodInfo _onDpiChangedMethod;
        private static MethodInfo _wmMoveChangedMethod;
        private static FieldInfo _uiElementDpiScaleXValuesField;
        private static FieldInfo _uiElementDpiScaleYValuesField;

        /// <summary>
        /// Sets application dpi scaling to specified value.
        /// </summary>
        public static void SetApplicationDpi(double dpiScale)
        {
            SetApplicationDpi(dpiScale, dpiScale);
        }

        /// <summary>
        /// Sets application dpi scaling to specified values.
        /// </summary>
        public static void SetApplicationDpi(double dpiScaleX, double dpiScaleY)
        {
            if (!_isInitialized)
            {
                Initialize();
            }

            // Cache user dpi scale as these values are going to be used whenever a new window opens.
            _dpiScaleX = dpiScaleX;
            _dpiScaleY = dpiScaleY;

            // Go through opened windows and set their dpi scale.
            foreach (Window window in Application.Current.Windows)
            {
                SetWindowDpiImpl(window, dpiScaleX, dpiScaleY);
            }

            // Set Dpi cache last as this changes what VisualTreeHelper.GetDpi method returns.
            SetUIElementStaticDpiCache(dpiScaleX, dpiScaleY);
        }

        /// <summary>
        /// Sets specified window dpi scaling to specified value.
        /// </summary>
        public static void SetWindowDpi(Window window, double dpiScale)
        {
            SetWindowDpi(window, dpiScale, dpiScale);
        }

        /// <summary>
        /// Sets specified window dpi scaling to specified values.
        /// </summary>
        public static void SetWindowDpi(Window window, double dpiScaleX, double dpiScaleY)
        {
            if (!_isInitialized)
            {
                Initialize();
            }

            // Set dpi scale of the specified window.
            SetWindowDpiImpl(window, dpiScaleX, dpiScaleY);
        }

        private static void Initialize()
        {
            // Get HwndSource.OnDpiChanged method. 
            // - Other option is to use HwndSource.ChangeDpi but that one has two overloads and the one we need calls OnDpiChanged anyway.
            _onDpiChangedMethod = typeof(HwndSource).GetMethod("OnDpiChanged", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
            // Get Window.WmMoveChanged method.
            _wmMoveChangedMethod = typeof(Window).GetMethod("WmMoveChanged", BindingFlags.NonPublic | BindingFlags.Instance);

            // Get static UIElement.DpiScaleXValues and DpiScaleYValues fields.
            // - These are used as data store for VisualTreeHelper.GetDpi.
            _uiElementDpiScaleXValuesField = typeof(UIElement).GetField("DpiScaleXValues", BindingFlags.Static | BindingFlags.NonPublic);
            _uiElementDpiScaleYValuesField = typeof(UIElement).GetField("DpiScaleYValues", BindingFlags.Static | BindingFlags.NonPublic);

            // Watch for new windows opening.
            EventManager.RegisterClassHandler(typeof(Window), Window.LoadedEvent, new RoutedEventHandler(OnWindowLoaded));
            // TODO: Watch for new popups opening.
            // Type popupRootType = typeof(FrameworkElement).Assembly.GetType("System.Windows.Controls.Primitives.PopupRoot");
            // EventManager.RegisterClassHandler(typeof(Window), Window.LoadedEvent, new RoutedEventHandler(OnPopupLoaded));

            _isInitialized = true;
        }

        private static void SetWindowDpiImpl(Window window, double dpiScaleX, double dpiScaleY)
        {
            // Get window current dpi.
            DpiScale dpi = VisualTreeHelper.GetDpi(window);
            // Prepare new window position.
            double left = window.Left * dpi.DpiScaleX;
            double top = window.Top * dpi.DpiScaleY;
            double width = window.ActualWidth * dpi.DpiScaleX;
            double height = window.ActualHeight * dpi.DpiScaleY;

            // Prepare an instance of HwndDpiChangedEventArgs.
            Type[] paramTypes = new Type[] { typeof(DpiScale), typeof(DpiScale), typeof(Rect) };
            object[] paramValues = new object[]
            { 
                    /* old dpi */ dpi, 
                    /* new dpi */ new DpiScale(dpiScaleX, dpiScaleY), 
                    /* position */ new Rect(left, top, width, height)
            };
            HwndDpiChangedEventArgs args = CreateInstance<HwndDpiChangedEventArgs>(paramTypes, paramValues);

            // Get window hosting HwndSource.
            PresentationSource source = PresentationSource.FromVisual(window);
            // Change DPI on the specified window.
            _onDpiChangedMethod.Invoke(source, new object[] { args });
            // Update Left and Top properties according to new DPI.
            _wmMoveChangedMethod.Invoke(window, null);
        }

        private static void SetUIElementStaticDpiCache(double dpiScaleX, double dpiScaleY)
        {
            List<double> scaleXValues = (List<double>)_uiElementDpiScaleXValuesField.GetValue(null);
            List<double> scaleYValues = (List<double>)_uiElementDpiScaleYValuesField.GetValue(null);

            for (int i = 0; i < scaleXValues.Count; i++)
            {
                scaleXValues[i] = dpiScaleX;
            }

            for (int i = 0; i < scaleYValues.Count; i++)
            {
                scaleYValues[i] = dpiScaleY;
            }
        }

        private static void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            SetWindowDpi((Window)sender, _dpiScaleX, _dpiScaleY);
        }

        private static T CreateInstance<T>(Type[] paramTypes, object[] paramValues)
        {
            Type t = typeof(T);

            ConstructorInfo ci =
                t.GetConstructor(
                    BindingFlags.Instance | BindingFlags.NonPublic,
                    null,
                    paramTypes,
                    null);

            return (T)ci.Invoke(paramValues);
        }
    }
}
