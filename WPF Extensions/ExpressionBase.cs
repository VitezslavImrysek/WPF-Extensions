using System.Runtime.CompilerServices;

// See the following nice blog post about IgnoresAccessChecksToAttribute:
// https://www.strathweb.com/2018/10/no-internalvisibleto-no-problem-bypassing-c-visibility-rules-with-roslyn/
[assembly: IgnoresAccessChecksTo("WindowsBase")]

namespace System.Windows.Extensions
{
    public class ExpressionBase : Expression
    {
        public ExpressionBase()
            : base()
        {

        }

        public ExpressionBase(ExpressionMode mode)
            : base((System.Windows.ExpressionMode)mode)
        {

        }

        public static object NoValuePublic => NoValue;

        public bool Attachable => base.Attachable;
        public bool Shareable => base.Shareable;
        public bool ForwardsInvalidations => base.ForwardsInvalidations;
        public bool SupportsUnboundSources => base.SupportsUnboundSources;
        public bool HasBeenAttached => base.HasBeenAttached;
        public bool HasBeenDetached => base.HasBeenDetached;

        // During compilation changed to internal override.
        private System.Windows.DependencySource[] GetSources()
        {
            return ToWPFDependencySource(GetSourcesBase());
        }

        internal virtual Expression Copy(DependencyObject targetObject, DependencyProperty targetDP)
        {
            return CopyBase(targetObject, targetDP);
        }

        internal virtual void OnAttach(DependencyObject d, DependencyProperty dp)
        {
            OnAttachBase(d, dp);
        }

        internal virtual void OnDetach(DependencyObject d, DependencyProperty dp)
        {
            OnDetachBase(d, dp);
        }

        internal virtual object GetValue(DependencyObject d, DependencyProperty dp)
        {
            return GetValueBase(d, dp);
        }

        internal virtual bool SetValue(DependencyObject d, DependencyProperty dp, object value)
        {
            return SetValueBase(d, dp, value);
        }

        internal virtual void OnPropertyInvalidation(DependencyObject d, DependencyPropertyChangedEventArgs args)
        {
            OnPropertyInvalidationBase(d, args);
        }

        public virtual DependencySource[] GetSourcesBase()
        {
            return null;
        }

        public virtual Expression CopyBase(DependencyObject targetObject, DependencyProperty targetDP)
        {
            // By default, just use the same copy.
            return this;
        }

        public virtual void OnAttachBase(DependencyObject d, DependencyProperty dp)
        {

        }

        public virtual void OnDetachBase(DependencyObject d, DependencyProperty dp)
        {

        }

        public virtual object GetValueBase(DependencyObject d, DependencyProperty dp)
        {
            return DependencyProperty.UnsetValue;
        }

        public virtual bool SetValueBase(DependencyObject d, DependencyProperty dp, object value)
        {
            return false;
        }

        public virtual void OnPropertyInvalidationBase(DependencyObject d, DependencyPropertyChangedEventArgs args)
        {

        }

        public void ChangeSources(DependencyObject d, DependencyProperty dp, DependencySource[] newSources)
        {
            base.ChangeSources(d, dp, ToWPFDependencySource(newSources));
        }

        public void MarkAttached()
        {
            base.MarkAttached();
        }

        public void MarkDetached()
        {
            base.MarkDetached();
        }

        private System.Windows.DependencySource[] ToWPFDependencySource(DependencySource[] sources)
        {
            if (sources == null)
            {
                return null;
            }

            System.Windows.DependencySource[] wpfSources = new System.Windows.DependencySource[sources.Length];
            for (int i = 0; i < sources.Length; i++)
            {
                DependencySource source = sources[i];
                wpfSources[i] = new System.Windows.DependencySource(source.DependencyObject, source.DependencyProperty);
            }

            return wpfSources;
        }
    }

    public enum ExpressionMode
    {
        None = 0,
        NonSharable = System.Windows.ExpressionMode.NonSharable,
        ForwardsInvalidations = System.Windows.ExpressionMode.ForwardsInvalidations,
        SupportsUnboundSources = System.Windows.ExpressionMode.SupportsUnboundSources
    }

    public sealed class DependencySource
    {
        public DependencySource(DependencyObject d, DependencyProperty dp)
        {
            DependencyObject = d;
            DependencyProperty = dp;
        }

        public DependencyObject DependencyObject { get; }
        public DependencyProperty DependencyProperty { get; }
    }
}

namespace System.Runtime.CompilerServices
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public class IgnoresAccessChecksToAttribute : Attribute
    {
        public IgnoresAccessChecksToAttribute(string assemblyName)
        {
            AssemblyName = assemblyName;
        }

        public string AssemblyName { get; }
    }
}