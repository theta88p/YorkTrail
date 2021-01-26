#pragma once
#define _USE_MATH_DEFINES
#include <cmath>
#include <vector>
#include <numeric>

class SampleRateConverter
{
private:
    static double sinc(double x);

public:
    static void Convert(double cur, double target, std::vector<float> &input, std::vector<float>& output, int samples);
};

