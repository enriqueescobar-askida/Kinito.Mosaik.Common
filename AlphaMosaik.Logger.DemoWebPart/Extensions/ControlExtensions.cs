using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI;

namespace AlphaMosaik.Logger.DemoWebPart.Extensions
{
    public static class ControlExtensions
    {
        public static Control FindControlRecursive(this Control ctl, string id)
        {
            Control returnControl = null;
            returnControl = ctl.FindControl(id);

            if(returnControl != null)
            {
                return returnControl;
            }
            else
            {
                foreach (Control childControl in ctl.Controls)
                {
                    returnControl = childControl.FindControlRecursive(id);

                    if(returnControl != null)
                    {
                        return returnControl;
                    }
                }
                return returnControl;
            }
        }
    }
}
