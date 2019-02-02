using System;
using System.Windows.Controls;

namespace Alphamosaik.Common.UI.Infrastructure
{
    public interface IVisibilityService
    {
        void EnterViewAnimation(UserControl view);
        void LeaveViewAnimation(UserControl view, Action onLeaveComplete);
    }
}