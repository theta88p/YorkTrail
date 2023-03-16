#include "FlacEncode.h"

int YorkTrail::FlacEncode::Init(std::string filePath)
{
    for (int i = 0; i < 5; i++)
    {
        auto enc = new FLAC::Encoder::File();
        enc->set_verify(false);
        enc->set_compression_level(5);
        enc->set_channels(2);
        enc->set_bits_per_sample(16);
        enc->set_sample_rate(44100);
        encoder.push_back(enc);
    }

    FLAC__StreamEncoderInitStatus status;
    status = encoder[0]->init(filePath + "\\vocals.flac");
    if (status != FLAC__STREAM_ENCODER_INIT_STATUS_OK)
    {
        return 1;
    }
    status = encoder[1]->init(filePath + "\\drums.flac");
    if (status != FLAC__STREAM_ENCODER_INIT_STATUS_OK)
    {
        return 1;
    }
    status = encoder[2]->init(filePath + "\\bass.flac");
    if (status != FLAC__STREAM_ENCODER_INIT_STATUS_OK)
    {
        return 1;
    }
    status = encoder[3]->init(filePath + "\\piano.flac");
    if (status != FLAC__STREAM_ENCODER_INIT_STATUS_OK)
    {
        return 1;
    }
    status = encoder[4]->init(filePath + "\\other.flac");
    if (status != FLAC__STREAM_ENCODER_INIT_STATUS_OK)
    {
        return 1;
    }
    return 0;
}

int YorkTrail::FlacEncode::Process(std::vector<std::vector<FLAC__int32>> &inputs, int64_t frameCount)
{
    std::vector<std::thread> thread;
    std::vector<bool> res(5);

    for (int i = 0; i < 5; i++)
    {
        thread.push_back(std::thread([&, i] { res[i] = encoder[i]->process_interleaved(inputs[i].data(), frameCount); }));
    }
    
    for (auto iter = thread.begin(); iter != thread.end(); ++iter)
    {
        iter->join();
    }

    thread.clear();

    for (int i = 0; i < 5; i++)
    {
        if (!res[i])
        {
            return 1;
        }
    }
    return 0;
}

void YorkTrail::FlacEncode::Uninit()
{
    for (int i = 0; i < 5; i++)
    {
        encoder[i]->finish();
        delete encoder[i];
    }

    encoder.clear();
}