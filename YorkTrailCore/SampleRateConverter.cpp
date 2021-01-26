#include "SampleRateConverter.h"

void SampleRateConverter::Convert(double cur, double target, std::vector<float> &input, std::vector<float>& output, int samples)
{
	int j = 32;
	int offset = j / 2;
	double d = target / cur;
	double acf = 0;

	if (!output.empty())
	{
		output.clear();
	}
	output.reserve(samples * (target / cur));

	for (int m = 0; m < samples * target / cur; m++)
	{
		double buff = 0;
		double t = m * cur / target;
		int n = (int)t;
		double dn = t - n;
		double wSum = 0;

		for (int k = -(j / 2); k <= (j / 2); k++)
		{
			//double window = 1;
			double window = 0.5 - 0.5 * cos(2.0 * M_PI * (((double)offset + k) / (j + 1)));
			//double window = 0.54 - 0.46 * cos(2.0 * M_PI * (((double)offset + k) / (j + 1)));
			//double window = 0.42 - 0.5 * cos(2.0 * M_PI * (((double)offset + k) / (j + 1))) + 0.08 * cos(4.0 * M_PI * (((double)offset + k) / (j + 1)));
			if (acf == 0)
			{
				wSum += window;
			}
			if (k + n >= 0 && k + n < samples)
			{
				if (target > cur)
				{
					// アップサンプリング
					buff += input[k + n] * sinc((double)k - dn) * window;
				}
				else
				{
					// ダウンサンプリング
					buff += d * input[k + n] * sinc(((double)k - dn) * d) * window;
				}
			}
		}
			
		if (acf == 0)
		{
			acf = 0.6 / (wSum / (j + 1));
		}
		output.push_back((float)(buff * acf));
	}
}

double SampleRateConverter::sinc(double x)
{
	if (x == 0)
	{
		return 1.0f;
	}
	else
	{
		return sin(M_PI * x) / (M_PI * x);
	}
}