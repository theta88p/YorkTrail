#include "RingBuffer.h"

// コンストラクタ
RingBuffer::RingBuffer()
{
	// 初期化を行う
	rpos = 0;
	wpos = RB_SIZE / 2; // とりあえずバッファサイズの半分ぐらいにしておく

	std::fill(buf.begin(), buf.end(), 0.0f);
}


// 読み込み位置と書き込み位置の間隔を設定する関数
void RingBuffer::SetInterval(int interval)
{
	// 読み込み位置と書き込み位置の間隔を設定

	// 値が0以下やバッファサイズ以上にならないよう処理
	interval = interval % RB_SIZE;
	if (interval <= 0) { interval = 1; }

	// 書き込み位置を読み込み位置からinterval分だけ離して設定
	wpos = (rpos + interval) % RB_SIZE;
}


// 内部バッファの読み込み位置(rpos)のデータを読み込む関数
float RingBuffer::Read(int pos)
{
	// 読み込み位置(rpos)と相対位置(pos)から実際に読み込む位置を計算する。
	int tmp = rpos + pos;

	while (tmp < 0)
	{
		tmp += RB_SIZE;
	}
	tmp = tmp % RB_SIZE; // バッファサイズ以上にならないよう処理

	// 読み込み位置の値を返す
	return buf[tmp];
}


// 内部バッファの書き込み位置(wpos)にデータを書き込む関数
void  RingBuffer::Write(float in)
{
	// 書き込み位置(wpos)に値を書き込む
	buf[wpos] = in;
}


// 内部バッファの読み込み位置(rpos)、書き込み位置(wpos)を一つ進める関数
void  RingBuffer::Update()
{
	// 内部バッファの読み込み位置(rpos)、書き込み位置(wpos)を一つ進める
	rpos = (rpos + 1) % RB_SIZE;
	wpos = (wpos + 1) % RB_SIZE;
}
