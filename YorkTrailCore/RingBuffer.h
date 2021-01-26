#pragma once
#ifndef __RINGBUF__
#define __RINGBUF__

#include <string.h>
#include <array>

// リングバッファのサイズ
// エフェクター(ディレイなど)で使用する想定なので
// とりあえず4秒分確保している(サンプリングレート 44,100Hz)
#define RB_SIZE (44100 * 4)

// ===================================================================================
// リングバッファクラス
// ===================================================================================
class RingBuffer
{
private:
	int rpos; // 読み込み位置
	int wpos; // 書き込み位置

	std::array<float, RB_SIZE> buf; // 内部バッファ

public:
	RingBuffer();

	// 読み込み位置と書き込み位置の間隔を設定する関数
	// ディレイエフェクターの場合はそのまま遅延時間(ディレイタイム)になる
	void SetInterval(int interval);

	// 内部バッファの読み込み位置(rpos)のデータを読み込む関数
	// 引数のposは読み込み位置(rpos)からの相対位置
	// (相対位置(pos)はコーラスやピッチシフタなどのエフェクターで利用する)
	float Read(int pos = 0);

	// 内部バッファの書き込み位置(wpos)にデータを書き込む関数
	void  Write(float in);

	// 内部バッファの読み込み位置(rpos)、書き込み位置(wpos)を一つ進める関数
	void Update();
};

#endif