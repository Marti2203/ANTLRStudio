/*
 * Copyright (c) 2012-2017 The ANTLR Project. All rights reserved.
 * Use of this file is governed by the BSD 3-clause license that
 * can be found in the LICENSE.txt file in the project root.
 */
using System;
using Eto.Drawing;

namespace ANTLRStudio.Trees
{
    public class EmptyIcon : Icon
    {
        public EmptyIcon() : base(0, new Bitmap(Array.Empty<byte>()))
        { }

    }
}