using System;
using ReddirWebApi.Models;

namespace ReddirWebApi.Services
{

    public interface IRedditAnalyzerService
    {
        Task<Dictionary<string, List<PostResultDto>>> AnalyzeAsync(AnalyzerRequestDto request);
    }

}