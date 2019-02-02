// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SecondSchedule.EventReceiver.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   Defines the SecondSchedule type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Alphamosaik.Translator.ApplicationFeatures.Features.Translator.UI
{
    using System;
    using System.Globalization;

    using Microsoft.SharePoint;

    public class SecondSchedule : SPSchedule
    {
        // Fields
        private int _secondInterval = 5;

        public override string Description
        {
            get
            {
                return "Minutes";
            }
        }

        // Methods
        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "every {0} seconds", new object[] { this._secondInterval });
        }

        // Properties
        public int Interval
        {
            get
            {
                return this._secondInterval;
            }

            set
            {
                if ((value < 0) || (value > 0x3b))
                {
                    throw new ArgumentOutOfRangeException("value");
                }

                this._secondInterval = value;
            }
        }
    }
}