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
#include "StemSeparator.h"

YorkTrail::StemSeparator::StemSeparator()
{
}

YorkTrail::StemSeparator::~StemSeparator()
{
}

std::error_code YorkTrail::StemSeparator::Init()
{
    std::error_code err;
    spleeter::Initialize(std::string("./models"), { spleeter::FiveStems }, err);
    return err;
}

void YorkTrail::StemSeparator::Uninit()
{
    auto pBundle = spleeter::Registry::instance().Get(spleeter::FiveStems);
    pBundle.get()->first.reset();
}

std::error_code YorkTrail::StemSeparator::Process(std::vector<float>& input, std::vector<std::vector<float>>& outputs, int frameCount)
{
    std::error_code err;
    auto source = Eigen::Map<spleeter::Waveform>(input.data(), 2, frameCount);
    spleeter::Waveform vocals, drums, bass, piano, other;
    spleeter::Split(source, &vocals, &drums, &bass, &piano, &other, err);
    Eigen::Map<spleeter::Waveform>(outputs[0].data(), 2, frameCount) = vocals;
    Eigen::Map<spleeter::Waveform>(outputs[1].data(), 2, frameCount) = drums;
    Eigen::Map<spleeter::Waveform>(outputs[2].data(), 2, frameCount) = bass;
    Eigen::Map<spleeter::Waveform>(outputs[3].data(), 2, frameCount) = piano;
    Eigen::Map<spleeter::Waveform>(outputs[4].data(), 2, frameCount) = other;

    return err;
}

std::error_code YorkTrail::StemSeparator::FilterInit()
{
    std::error_code err;
    filter->set_extra_frame_latency(10);
    filter->Init(err);

    if (err)
    {
        return err;
    }

    filter->set_volume(0, 1.0);
    filter->set_volume(1, 1.0);
    filter->set_OverlapLength(2);
    filter->set_FrameLength(filter->ProcessLength() - filter->OverlapLength());

    return err;
}

void YorkTrail::StemSeparator::FilterProcess(std::vector<float> &input, int frameCount)
{
    filter->set_block_size(frameCount);
    rtff::AudioBuffer buffer(frameCount, filter->channel_count());
    buffer.fromInterleaved(input.data());
    filter->ProcessBlock(&buffer);
    buffer.toInterleaved(input.data());
}