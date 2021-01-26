#include "RingBuffer.h"

// �R���X�g���N�^
RingBuffer::RingBuffer()
{
	// ���������s��
	rpos = 0;
	wpos = RB_SIZE / 2; // �Ƃ肠�����o�b�t�@�T�C�Y�̔������炢�ɂ��Ă���

	std::fill(buf.begin(), buf.end(), 0.0f);
}


// �ǂݍ��݈ʒu�Ə������݈ʒu�̊Ԋu��ݒ肷��֐�
void RingBuffer::SetInterval(int interval)
{
	// �ǂݍ��݈ʒu�Ə������݈ʒu�̊Ԋu��ݒ�

	// �l��0�ȉ���o�b�t�@�T�C�Y�ȏ�ɂȂ�Ȃ��悤����
	interval = interval % RB_SIZE;
	if (interval <= 0) { interval = 1; }

	// �������݈ʒu��ǂݍ��݈ʒu����interval�����������Đݒ�
	wpos = (rpos + interval) % RB_SIZE;
}


// �����o�b�t�@�̓ǂݍ��݈ʒu(rpos)�̃f�[�^��ǂݍ��ފ֐�
float RingBuffer::Read(int pos)
{
	// �ǂݍ��݈ʒu(rpos)�Ƒ��Έʒu(pos)������ۂɓǂݍ��ވʒu���v�Z����B
	int tmp = rpos + pos;

	while (tmp < 0)
	{
		tmp += RB_SIZE;
	}
	tmp = tmp % RB_SIZE; // �o�b�t�@�T�C�Y�ȏ�ɂȂ�Ȃ��悤����

	// �ǂݍ��݈ʒu�̒l��Ԃ�
	return buf[tmp];
}


// �����o�b�t�@�̏������݈ʒu(wpos)�Ƀf�[�^���������ފ֐�
void  RingBuffer::Write(float in)
{
	// �������݈ʒu(wpos)�ɒl����������
	buf[wpos] = in;
}


// �����o�b�t�@�̓ǂݍ��݈ʒu(rpos)�A�������݈ʒu(wpos)����i�߂�֐�
void  RingBuffer::Update()
{
	// �����o�b�t�@�̓ǂݍ��݈ʒu(rpos)�A�������݈ʒu(wpos)����i�߂�
	rpos = (rpos + 1) % RB_SIZE;
	wpos = (wpos + 1) % RB_SIZE;
}
