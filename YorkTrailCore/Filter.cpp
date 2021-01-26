#include "Filter.h"

// --------------------------------------------------------------------------------
// �R���X�g���N�^
// --------------------------------------------------------------------------------
Filter::Filter()
{
	Flush();
}

void Filter::Flush()
{
	// �����o�[�ϐ���������
	a0 = 1.0f; // 0�ȊO�ɂ��Ă����Ȃ��Ə��Z�ŃG���[�ɂȂ�
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
// ���͐M���Ƀt�B���^��K�p����֐�
// --------------------------------------------------------------------------------
float Filter::Process(float in)
{
	// ���͐M���Ƀt�B���^��K�p���A�o�͐M���ϐ��ɕۑ��B
	float out = b0 / a0 * in + b1 / a0 * in1 + b2 / a0 * in2
		- a1 / a0 * out1 - a2 / a0 * out2;

	in2 = in1; // 2�O�̓��͐M�����X�V
	in1 = in;  // 1�O�̓��͐M�����X�V

	out2 = out1; // 2�O�̏o�͐M�����X�V
	out1 = out;  // 1�O�̏o�͐M�����X�V

	// �o�͐M����Ԃ�
	return out;
}

// --------------------------------------------------------------------------------
// �t�B���^�W�����v�Z���郁���o�[�֐�
// --------------------------------------------------------------------------------
void Filter::LowPass(float freq, float q, float samplerate)
{
	// �t�B���^�W���v�Z�Ŏg�p���钆�Ԓl�����߂�B
	float omega = 2.0f * M_PI *  freq / samplerate;
	float alpha = sin(omega) / (2.0f * q);

	// �t�B���^�W�������߂�B
	a0 = 1.0f + alpha;
	a1 = -2.0f * cos(omega);
	a2 = 1.0f - alpha;
	b0 = (1.0f - cos(omega)) / 2.0f;
	b1 = 1.0f - cos(omega);
	b2 = (1.0f - cos(omega)) / 2.0f;
}

void Filter::HighPass(float freq, float q, float samplerate)
{
	// �t�B���^�W���v�Z�Ŏg�p���钆�Ԓl�����߂�B
	float omega = 2.0f * M_PI *  freq / samplerate;
	float alpha = sin(omega) / (2.0f * q);

	// �t�B���^�W�������߂�B
	a0 = 1.0f + alpha;
	a1 = -2.0f * cos(omega);
	a2 = 1.0f - alpha;
	b0 = (1.0f + cos(omega)) / 2.0f;
	b1 = -(1.0f + cos(omega));
	b2 = (1.0f + cos(omega)) / 2.0f;
}

void Filter::BandPass(float freq, float bw, float samplerate)
{
	// �t�B���^�W���v�Z�Ŏg�p���钆�Ԓl�����߂�B
	float omega = 2.0f * M_PI *  freq / samplerate;
	float alpha = sin(omega) * sinh(log(2.0f) / 2.0 * bw * omega / sin(omega));

	// �t�B���^�W�������߂�B
	a0 = 1.0f + alpha;
	a1 = -2.0f * cos(omega);
	a2 = 1.0f - alpha;
	b0 = alpha;
	b1 = 0.0f;
	b2 = -alpha;
}

void Filter::Notch(float freq, float bw, float samplerate)
{
	// �t�B���^�W���v�Z�Ŏg�p���钆�Ԓl�����߂�B
	float omega = 2.0f * M_PI *  freq / samplerate;
	float alpha = sin(omega) * sinh(log(2.0f) / 2.0 * bw * omega / sin(omega));

	// �t�B���^�W�������߂�B
	a0 = 1.0f + alpha;
	a1 = -2.0f * cos(omega);
	a2 = 1.0f - alpha;
	b0 = 1.0f;
	b1 = -2.0f * cos(omega);
	b2 = 1.0f;
}

void Filter::LowShelf(float freq, float q, float gain, float samplerate)
{
	// �t�B���^�W���v�Z�Ŏg�p���钆�Ԓl�����߂�B
	float omega = 2.0f * M_PI *  freq / samplerate;
	float alpha = sin(omega) / (2.0f * q);
	float A = pow(10.0f, (gain / 40.0f));
	float beta = sqrt(A) / q;

	// �t�B���^�W�������߂�B
	a0 = (A + 1.0f) + (A - 1.0f) * cos(omega) + beta * sin(omega);
	a1 = -2.0f * ((A - 1.0f) + (A + 1.0f) * cos(omega));
	a2 = (A + 1.0f) + (A - 1.0f) * cos(omega) - beta * sin(omega);
	b0 = A * ((A + 1.0f) - (A - 1.0f) * cos(omega) + beta * sin(omega));
	b1 = 2.0f * A * ((A - 1.0f) - (A + 1.0f) * cos(omega));
	b2 = A * ((A + 1.0f) - (A - 1.0f) * cos(omega) - beta * sin(omega));
}

void Filter::HighShelf(float freq, float q, float gain, float samplerate)
{
	// �t�B���^�W���v�Z�Ŏg�p���钆�Ԓl�����߂�B
	float omega = 2.0f * M_PI *  freq / samplerate;
	float alpha = sin(omega) / (2.0f * q);
	float A = pow(10.0f, (gain / 40.0f));
	float beta = sqrt(A) / q;

	// �t�B���^�W�������߂�B
	a0 = (A + 1.0f) - (A - 1.0f) * cos(omega) + beta * sin(omega);
	a1 = 2.0f * ((A - 1.0f) - (A + 1.0f) * cos(omega));
	a2 = (A + 1.0f) - (A - 1.0f) * cos(omega) - beta * sin(omega);
	b0 = A * ((A + 1.0f) + (A - 1.0f) * cos(omega) + beta * sin(omega));
	b1 = -2.0f * A * ((A - 1.0f) + (A + 1.0f) * cos(omega));
	b2 = A * ((A + 1.0f) + (A - 1.0f) * cos(omega) - beta * sin(omega));
}


void Filter::Peaking(float freq, float bw, float gain, float samplerate)
{
	// �t�B���^�W���v�Z�Ŏg�p���钆�Ԓl�����߂�B
	float omega = 2.0f * M_PI *  freq / samplerate;
	float alpha = sin(omega) * sinh(log(2.0f) / 2.0 * bw * omega / sin(omega));
	float A = pow(10.0f, (gain / 40.0f));

	// �t�B���^�W�������߂�B
	a0 = 1.0f + alpha / A;
	a1 = -2.0f * cos(omega);
	a2 = 1.0f - alpha / A;
	b0 = 1.0f + alpha * A;
	b1 = -2.0f * cos(omega);
	b2 = 1.0f - alpha * A;
}

void Filter::AllPass(float freq, float q, float samplerate)
{
	// �t�B���^�W���v�Z�Ŏg�p���钆�Ԓl�����߂�B
	float omega = 2.0f * M_PI *  freq / samplerate;
	float alpha = sin(omega) / (2.0f * q);

	// �t�B���^�W�������߂�B
	a0 = 1.0f + alpha;
	a1 = -2.0f * cos(omega);
	a2 = 1.0f - alpha;
	b0 = 1.0f - alpha;
	b1 = -2.0f * cos(omega);
	b2 = 1.0f + alpha;
}