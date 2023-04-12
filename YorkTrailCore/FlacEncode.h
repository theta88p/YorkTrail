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
		int InitOne(std::string filePath);
		int ProcessOne(std::vector<FLAC__int32>& input, int64_t frameCount);
		void UninitOne();
	private:
		std::vector<FLAC::Encoder::File*> encoder;
	};

}