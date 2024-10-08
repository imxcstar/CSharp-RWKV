﻿using RWKVSharp.Core.Sampler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RWKVSharp.Core.Interfaces
{
    public interface IRunner
    {
        public string Name { get; }
        public IModel Model { get; }
        public IRunnerOptions Options { get; set; }

        public void Init(string name, IModel model, IRunnerOptions options);

        public void InitInstruction(string instruction);
        public void Generate(string value, Action<string> callBack, RunOptions? options = null);
        public string Generate(string value, RunOptions? options = null);
        public IAsyncEnumerable<string> GenerateAsync(string value, RunOptions? options = null, CancellationToken cancellationToken = default);
    }

    public class RunOptions
    {
        public int MaxTokens { get; set; } = 512;
        public ISampler? Sampler { get; set; }
    }
}
