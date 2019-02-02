using System;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace Alphamosaik.Common.UI.Infrastructure.Behaviors
{
    public class MouseWheelScrollBehavior : Behavior<Control>
    {
        /// <summary>
        /// Gets or sets the peer.
        /// </summary>
        /// <value>The peer.</value>
        private AutomationPeer Peer { get; set; }

        /// <summary>
        /// Called after the behavior is attached to an AssociatedObject.
        /// </summary>
        /// <remarks>Override this to hook up functionality to the AssociatedObject.</remarks>
        protected override void OnAttached()
        {
            this.Peer = FrameworkElementAutomationPeer.FromElement(this.AssociatedObject);

            if (this.Peer == null)
                this.Peer = FrameworkElementAutomationPeer.CreatePeerForElement(this.AssociatedObject);

            this.AssociatedObject.MouseWheel += new MouseWheelEventHandler(AssociatedObject_MouseWheel);
            base.OnAttached();
        }

        /// <summary>
        /// Called when the behavior is being detached from its AssociatedObject, but before it has actually occurred.
        /// </summary>
        /// <remarks>Override this to unhook functionality from the AssociatedObject.</remarks>
        protected override void OnDetaching()
        {
            this.AssociatedObject.MouseWheel -= new MouseWheelEventHandler(AssociatedObject_MouseWheel);
            base.OnDetaching();
        }

        /// <summary>
        /// Handles the MouseWheel event of the AssociatedObject control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.MouseWheelEventArgs"/> instance containing the event data.</param>
        void AssociatedObject_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            this.AssociatedObject.Focus();

            int direction = Math.Sign(e.Delta);

            ScrollAmount scrollAmount =
                (direction < 0) ? ScrollAmount.SmallIncrement : ScrollAmount.SmallDecrement;

            if (this.Peer != null)
            {
                IScrollProvider scrollProvider =
                    this.Peer.GetPattern(PatternInterface.Scroll) as IScrollProvider;

                bool shiftKey = (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift;

                if (scrollProvider != null && scrollProvider.VerticallyScrollable && !shiftKey)
                    scrollProvider.Scroll(ScrollAmount.NoAmount, scrollAmount);
                else if (scrollProvider != null && scrollProvider.VerticallyScrollable && shiftKey)
                    scrollProvider.Scroll(scrollAmount, ScrollAmount.NoAmount);
            }
        }
    }
}