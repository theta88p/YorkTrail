#include "Utils.h"

std::string Utils::Cli2Native(System::String^ src)
{
    // C# の文字列をCLIのバイト配列に変換
    auto utf8Array = Encoding::UTF8->GetBytes(src);

    // CLIのバイト配列をC++の配列に変換
    unsigned char* nativeArray = new unsigned char[utf8Array->Length];
    for (int i = 0; i < utf8Array->Length; i++)
    {
        nativeArray[i] = utf8Array[i];
    }

    // C++ の文字列型にデータを設定
    std::string nativeString(reinterpret_cast<char const*>(nativeArray), utf8Array->Length);
    for (int i = 0; i < utf8Array->Length; i++)
    {
        nativeArray[i] = '\0';
    }

    // 後片付け
    delete[] nativeArray;
    return nativeString;
}