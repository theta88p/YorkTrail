#pragma once
#include <vector>
#include <thread>
#include "FLAC++/encoder.h"

namespace YorkTrail
{
	static class FlacEncode
	{
	public:
		int Init(std::string filePath);
		int Process(std::vector<std::vector<FLAC__int32>> &inputs, int64_t frameCount);
		void Uninit();
	private:
		std::vector<FLAC::Encoder::File*> encoder;
	};

}