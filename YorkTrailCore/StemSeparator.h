/*
    YorkTrail
    Copyright (C) 2021 theta

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/
#pragma once
#include <system_error>
#include <vector>

#include "spleeter/spleeter.h"
#include "spleeter_filter/filter.h"
#include "spleeter_common/spleeter_common.h"
#include "spleeter_common/registry.h"
#include "artff/abstract_filter.h"

namespace YorkTrail
{
	class StemSeparator
	{
	public:
		StemSeparator();
		~StemSeparator();

		std::error_code Init();
		void Uninit();
		std::error_code FilterInit();
		std::error_code Process(std::vector<float>& input, std::vector<std::vector<float>>& outputs, int frameCount);
		void FilterProcess(std::vector<float> &input, int frameCount);

		spleeter::Filter* filter;
	};

}