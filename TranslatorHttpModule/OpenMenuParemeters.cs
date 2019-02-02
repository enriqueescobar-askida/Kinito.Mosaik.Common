// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OpenMenuParemeters.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   Defines the OpenMenuParemeters type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace TranslatorHttpHandler
{
    public class OpenMenuParemeters
    {
        private readonly string _webpartName;
        private readonly string _menuName;
        private readonly bool _mergeMenu;

        public OpenMenuParemeters(string webpartName, string menuName, bool mergeMenu)
        {
            _webpartName = webpartName;
            _mergeMenu = mergeMenu;
            _menuName = menuName;
        }

        public bool MergeMenu
        {
            get { return _mergeMenu; }
        }

        public string MenuName
        {
            get { return _menuName; }
        }

        public string WebpartName
        {
            get { return _webpartName; }
        }
    }
}
