#pragma once
#include <string>

using namespace System::Text;

ref class Utils
{
public:
	static std::string Cli2Native(System::String^ src);
};

