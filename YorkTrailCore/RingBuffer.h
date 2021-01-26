#pragma once
#ifndef __RINGBUF__
#define __RINGBUF__

#include <string.h>
#include <array>

// �����O�o�b�t�@�̃T�C�Y
// �G�t�F�N�^�[(�f�B���C�Ȃ�)�Ŏg�p����z��Ȃ̂�
// �Ƃ肠����4�b���m�ۂ��Ă���(�T���v�����O���[�g 44,100Hz)
#define RB_SIZE (44100 * 4)

// ===================================================================================
// �����O�o�b�t�@�N���X
// ===================================================================================
class RingBuffer
{
private:
	int rpos; // �ǂݍ��݈ʒu
	int wpos; // �������݈ʒu

	std::array<float, RB_SIZE> buf; // �����o�b�t�@

public:
	RingBuffer();

	// �ǂݍ��݈ʒu�Ə������݈ʒu�̊Ԋu��ݒ肷��֐�
	// �f�B���C�G�t�F�N�^�[�̏ꍇ�͂��̂܂ܒx������(�f�B���C�^�C��)�ɂȂ�
	void SetInterval(int interval);

	// �����o�b�t�@�̓ǂݍ��݈ʒu(rpos)�̃f�[�^��ǂݍ��ފ֐�
	// ������pos�͓ǂݍ��݈ʒu(rpos)����̑��Έʒu
	// (���Έʒu(pos)�̓R�[���X��s�b�`�V�t�^�Ȃǂ̃G�t�F�N�^�[�ŗ��p����)
	float Read(int pos = 0);

	// �����o�b�t�@�̏������݈ʒu(wpos)�Ƀf�[�^���������ފ֐�
	void  Write(float in);

	// �����o�b�t�@�̓ǂݍ��݈ʒu(rpos)�A�������݈ʒu(wpos)����i�߂�֐�
	void Update();
};

#endif