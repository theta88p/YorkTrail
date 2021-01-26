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
	// フィルタの係数
	float a0, a1, a2, b0, b1, b2;
	// バッファ
	float out1, out2;
	float in1, in2;

public:
	Filter();

	// 入力信号にフィルタを適用する関数
	float Process(float in);
	void Flush();

	// フィルタ係数を計算するメンバー関数
	void LowPass(float freq, float q, float samplerate);
	void HighPass(float freq, float q, float samplerate);
	void BandPass(float freq, float bw, float samplerate);
	void Notch(float freq, float bw, float samplerate);
	void LowShelf(float freq, float q, float gain, float samplerate);
	void HighShelf(float freq, float q, float gain, float samplerate);
	void Peaking(float freq, float bw, float gain, float samplerate);
	void AllPass(float freq, float q, float samplerate);
};