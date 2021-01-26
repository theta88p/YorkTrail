#include "Filter.h"

// --------------------------------------------------------------------------------
// コンストラクタ
// --------------------------------------------------------------------------------
Filter::Filter()
{
	Flush();
}

void Filter::Flush()
{
	// メンバー変数を初期化
	a0 = 1.0f; // 0以外にしておかないと除算でエラーになる
	a1 = 0.0f;
	a2 = 0.0f;
	b0 = 1.0f;
	b1 = 0.0f;
	b2 = 0.0f;

	in1 = 0.0f;
	in2 = 0.0f;

	out1 = 0.0f;
	out2 = 0.0f;
}

// --------------------------------------------------------------------------------
// 入力信号にフィルタを適用する関数
// --------------------------------------------------------------------------------
float Filter::Process(float in)
{
	// 入力信号にフィルタを適用し、出力信号変数に保存。
	float out = b0 / a0 * in + b1 / a0 * in1 + b2 / a0 * in2
		- a1 / a0 * out1 - a2 / a0 * out2;

	in2 = in1; // 2つ前の入力信号を更新
	in1 = in;  // 1つ前の入力信号を更新

	out2 = out1; // 2つ前の出力信号を更新
	out1 = out;  // 1つ前の出力信号を更新

	// 出力信号を返す
	return out;
}

// --------------------------------------------------------------------------------
// フィルタ係数を計算するメンバー関数
// --------------------------------------------------------------------------------
void Filter::LowPass(float freq, float q, float samplerate)
{
	// フィルタ係数計算で使用する中間値を求める。
	float omega = 2.0f * M_PI *  freq / samplerate;
	float alpha = sin(omega) / (2.0f * q);

	// フィルタ係数を求める。
	a0 = 1.0f + alpha;
	a1 = -2.0f * cos(omega);
	a2 = 1.0f - alpha;
	b0 = (1.0f - cos(omega)) / 2.0f;
	b1 = 1.0f - cos(omega);
	b2 = (1.0f - cos(omega)) / 2.0f;
}

void Filter::HighPass(float freq, float q, float samplerate)
{
	// フィルタ係数計算で使用する中間値を求める。
	float omega = 2.0f * M_PI *  freq / samplerate;
	float alpha = sin(omega) / (2.0f * q);

	// フィルタ係数を求める。
	a0 = 1.0f + alpha;
	a1 = -2.0f * cos(omega);
	a2 = 1.0f - alpha;
	b0 = (1.0f + cos(omega)) / 2.0f;
	b1 = -(1.0f + cos(omega));
	b2 = (1.0f + cos(omega)) / 2.0f;
}

void Filter::BandPass(float freq, float bw, float samplerate)
{
	// フィルタ係数計算で使用する中間値を求める。
	float omega = 2.0f * M_PI *  freq / samplerate;
	float alpha = sin(omega) * sinh(log(2.0f) / 2.0 * bw * omega / sin(omega));

	// フィルタ係数を求める。
	a0 = 1.0f + alpha;
	a1 = -2.0f * cos(omega);
	a2 = 1.0f - alpha;
	b0 = alpha;
	b1 = 0.0f;
	b2 = -alpha;
}

void Filter::Notch(float freq, float bw, float samplerate)
{
	// フィルタ係数計算で使用する中間値を求める。
	float omega = 2.0f * M_PI *  freq / samplerate;
	float alpha = sin(omega) * sinh(log(2.0f) / 2.0 * bw * omega / sin(omega));

	// フィルタ係数を求める。
	a0 = 1.0f + alpha;
	a1 = -2.0f * cos(omega);
	a2 = 1.0f - alpha;
	b0 = 1.0f;
	b1 = -2.0f * cos(omega);
	b2 = 1.0f;
}

void Filter::LowShelf(float freq, float q, float gain, float samplerate)
{
	// フィルタ係数計算で使用する中間値を求める。
	float omega = 2.0f * M_PI *  freq / samplerate;
	float alpha = sin(omega) / (2.0f * q);
	float A = pow(10.0f, (gain / 40.0f));
	float beta = sqrt(A) / q;

	// フィルタ係数を求める。
	a0 = (A + 1.0f) + (A - 1.0f) * cos(omega) + beta * sin(omega);
	a1 = -2.0f * ((A - 1.0f) + (A + 1.0f) * cos(omega));
	a2 = (A + 1.0f) + (A - 1.0f) * cos(omega) - beta * sin(omega);
	b0 = A * ((A + 1.0f) - (A - 1.0f) * cos(omega) + beta * sin(omega));
	b1 = 2.0f * A * ((A - 1.0f) - (A + 1.0f) * cos(omega));
	b2 = A * ((A + 1.0f) - (A - 1.0f) * cos(omega) - beta * sin(omega));
}

void Filter::HighShelf(float freq, float q, float gain, float samplerate)
{
	// フィルタ係数計算で使用する中間値を求める。
	float omega = 2.0f * M_PI *  freq / samplerate;
	float alpha = sin(omega) / (2.0f * q);
	float A = pow(10.0f, (gain / 40.0f));
	float beta = sqrt(A) / q;

	// フィルタ係数を求める。
	a0 = (A + 1.0f) - (A - 1.0f) * cos(omega) + beta * sin(omega);
	a1 = 2.0f * ((A - 1.0f) - (A + 1.0f) * cos(omega));
	a2 = (A + 1.0f) - (A - 1.0f) * cos(omega) - beta * sin(omega);
	b0 = A * ((A + 1.0f) + (A - 1.0f) * cos(omega) + beta * sin(omega));
	b1 = -2.0f * A * ((A - 1.0f) + (A + 1.0f) * cos(omega));
	b2 = A * ((A + 1.0f) + (A - 1.0f) * cos(omega) - beta * sin(omega));
}


void Filter::Peaking(float freq, float bw, float gain, float samplerate)
{
	// フィルタ係数計算で使用する中間値を求める。
	float omega = 2.0f * M_PI *  freq / samplerate;
	float alpha = sin(omega) * sinh(log(2.0f) / 2.0 * bw * omega / sin(omega));
	float A = pow(10.0f, (gain / 40.0f));

	// フィルタ係数を求める。
	a0 = 1.0f + alpha / A;
	a1 = -2.0f * cos(omega);
	a2 = 1.0f - alpha / A;
	b0 = 1.0f + alpha * A;
	b1 = -2.0f * cos(omega);
	b2 = 1.0f - alpha * A;
}

void Filter::AllPass(float freq, float q, float samplerate)
{
	// フィルタ係数計算で使用する中間値を求める。
	float omega = 2.0f * M_PI *  freq / samplerate;
	float alpha = sin(omega) / (2.0f * q);

	// フィルタ係数を求める。
	a0 = 1.0f + alpha;
	a1 = -2.0f * cos(omega);
	a2 = 1.0f - alpha;
	b0 = 1.0f - alpha;
	b1 = -2.0f * cos(omega);
	b2 = 1.0f + alpha;
}