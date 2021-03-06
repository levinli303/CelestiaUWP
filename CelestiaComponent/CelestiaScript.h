//
// Copyright © 2021 Celestia Development Team. All rights reserved.
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either version 2
// of the License, or (at your option) any later version.
//

#pragma once

#include <celestia/scriptmenu.h>
#include "CelestiaScript.g.h"

namespace winrt::CelestiaComponent::implementation
{
    struct CelestiaScript : CelestiaScriptT<CelestiaScript>
    {
        CelestiaScript(ScriptMenuItem const& script);

        hstring Filename();
        hstring Title();

        hstring filename;
        hstring title;
    };
}
