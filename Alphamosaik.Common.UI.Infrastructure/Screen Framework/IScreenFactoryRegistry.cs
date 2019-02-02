using System;

namespace Alphamosaik.Common.UI.Infrastructure
{
    public interface IScreenFactoryRegistry
    {
        IScreenFactory Get(ScreenKeyType screenType);
        void Register(ScreenKeyType screenType, Type screenFactory);
        bool HasFactory(ScreenKeyType screenType);
    }
}