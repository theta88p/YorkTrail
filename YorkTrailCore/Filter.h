#pragma once
#define _USE_MATH_DEFINES
#include <cmath>
#include <math.h>


// --------------------------------------------------------------------------------
// CFilter
// --------------------------------------------------------------------------------
class Filter
{
private:
	// �t�B���^�̌W��
	float a0, a1, a2, b0, b1, b2;
	// �o�b�t�@
	float out1, out2;
	float in1, in2;

public:
	Filter();

	// ���͐M���Ƀt�B���^��K�p����֐�
	float Process(float in);
	void Flush();

	// �t�B���^�W�����v�Z���郁���o�[�֐�
	void LowPass(float freq, float q, float samplerate);
	void HighPass(float freq, float q, float samplerate);
	void BandPass(float freq, float bw, float samplerate);
	void Notch(float freq, float bw, float samplerate);
	void LowShelf(float freq, float q, float gain, float samplerate);
	void HighShelf(float freq, float q, float gain, float samplerate);
	void Peaking(float freq, float bw, float gain, float samplerate);
	void AllPass(float freq, float q, float samplerate);
};