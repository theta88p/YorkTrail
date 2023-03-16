#include "Utils.h"

std::string Utils::Cli2Native(System::String^ src)
{
    // C# �̕������CLI�̃o�C�g�z��ɕϊ�
    auto utf8Array = Encoding::UTF8->GetBytes(src);

    // CLI�̃o�C�g�z���C++�̔z��ɕϊ�
    unsigned char* nativeArray = new unsigned char[utf8Array->Length];
    for (int i = 0; i < utf8Array->Length; i++)
    {
        nativeArray[i] = utf8Array[i];
    }

    // C++ �̕�����^�Ƀf�[�^��ݒ�
    std::string nativeString(reinterpret_cast<char const*>(nativeArray), utf8Array->Length);
    for (int i = 0; i < utf8Array->Length; i++)
    {
        nativeArray[i] = '\0';
    }

    // ��Еt��
    delete[] nativeArray;
    return nativeString;
}