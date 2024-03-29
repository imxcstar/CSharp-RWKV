﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RWKV
{
    public static partial class RwkvCppNative
    {
        private const string LIBRARY_NAME = "librwkv";

        /// <summary>
        /// Represents an error encountered during a function call.
        /// These are flags, so an actual value might contain multiple errors.
        /// </summary>
        public enum RwkvErrorFlags
        {
            None = 0,
            Args = 1 << 8,
            File = 2 << 8,
            Model = 3 << 8,
            ModelParams = 4 << 8,
            Graph = 5 << 8,
            Ctx = 6 << 8,
            Alloc = 1,
            FileOpen = 2,
            FileStat = 3,
            FileRead = 4,
            FileWrite = 5,
            FileMagic = 6,
            FileVersion = 7,
            DataType = 8,
            Unsupported = 9,
            Shape = 10,
            Dimension = 11,
            Key = 12,
            Data = 13,
            ParamMissing = 14
        }

        /// <summary>
        /// Sets whether errors are automatically printed to stderr.
        /// If this is set to false, you are responsible for calling rwkv_last_error manually if an operation fails.
        /// </summary>
        /// <param name="ctx">
        ///   the context to suppress error messages for.
        ///   If NULL, affects model load (rwkv_init_from_file) and quantization (rwkv_quantize_model_file) errors,
        ///   as well as the default for new context.
        /// </param>
        /// <param name="print_errors">
        ///   whether error messages should be automatically printed.
        /// </param>
        /// <returns></returns>
#if NET7_0_OR_GREATER
        [LibraryImport(LIBRARY_NAME)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial void rwkv_set_print_errors(IntPtr ctx, [MarshalAs(UnmanagedType.I1)] bool print_errors);
#else
        [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern void rwkv_set_print_errors(IntPtr ctx, bool print_errors);
#endif

        /// <summary>
        /// Gets whether errors are automatically printed to stderr.
        /// </summary>
        /// <param name="ctx">the context to retrieve the setting for, or NULL for the global setting.</param>
        /// <returns></returns>
#if NET7_0_OR_GREATER
        [LibraryImport(LIBRARY_NAME)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        [return: MarshalAs(UnmanagedType.I1)]
        public static partial bool rwkv_get_print_errors(IntPtr ctx);
#else
        [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool rwkv_get_print_errors(IntPtr ctx);
#endif

        /// <summary>
        /// Retrieves and clears the error flags.
        /// </summary>
        /// <param name="ctx">the context the retrieve the error for, or NULL for the global error.</param>
        /// <returns></returns>
#if NET7_0_OR_GREATER
        [LibraryImport(LIBRARY_NAME)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial RwkvErrorFlags rwkv_get_last_error(IntPtr ctx);
#else
        [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern RwkvErrorFlags rwkv_get_last_error(IntPtr ctx);
#endif

        /// <summary>
        /// Loads the model from a file and prepares it for inference.
        /// Returns NULL on any error.
        /// </summary>
        /// <param name="model_file_path">path to model file in ggml format.</param>
        /// <param name="n_threads">count of threads to use, must be positive.</param>
        /// <returns></returns>
#if NET7_0_OR_GREATER
        [LibraryImport(LIBRARY_NAME, StringMarshalling = StringMarshalling.Utf8)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial IntPtr rwkv_init_from_file(string model_file_path, uint n_threads);
#else
        [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr rwkv_init_from_file(string model_file_path, uint n_threads);
#endif

        /// <summary>
        /// Creates a new context from an existing one.
        /// This can allow you to run multiple rwkv_eval's in parallel, without having to load a single model multiple times.
        /// Each rwkv_context can have one eval running at a time.
        /// Every rwkv_context must be freed using rwkv_free.
        /// </summary>
        /// <param name="ctx">context to be cloned.</param>
        /// <param name="n_threads">count of threads to use, must be positive.</param>
        /// <returns></returns>
#if NET7_0_OR_GREATER
        [LibraryImport(LIBRARY_NAME)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial IntPtr rwkv_clone_context(IntPtr ctx, uint n_threads);
#else
        [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr rwkv_clone_context(IntPtr ctx, uint n_threads);
#endif

        /// <summary>
        /// Offloads specified count of model layers onto the GPU. Offloaded layers are evaluated using cuBLAS or CLBlast.
        /// For the purposes of this function, model head (unembedding matrix) is treated as an additional layer:
        /// - pass `rwkv_get_n_layer(ctx)` to offload all layers except model head
        /// - pass `rwkv_get_n_layer(ctx) + 1` to offload all layers, including model head
        /// Returns true if at least one layer was offloaded.
        /// If rwkv.cpp was compiled without cuBLAS and CLBlast support, this function is a no-op and always returns false.
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="n_layers"></param>
        /// <returns></returns>
#if NET7_0_OR_GREATER
        [LibraryImport(LIBRARY_NAME)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        [return: MarshalAs(UnmanagedType.I1)]
        public static partial bool rwkv_gpu_offload_layers(IntPtr ctx, uint n_layers);
#else
        [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool rwkv_gpu_offload_layers(IntPtr ctx, uint n_layers);
#endif

        /// <summary>
        /// Evaluates the model for a single token.
        /// You can pass NULL to logits_out whenever logits are not needed. This can improve speed by ~10 ms per iteration, because logits are not calculated.
        /// Not thread-safe. For parallel inference, call rwkv_clone_context to create one rwkv_context for each thread.
        /// Returns false on any error.
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="token">next token index, in range 0 <= token < n_vocab.</param>
        /// <param name="state_in">FP32 buffer of size rwkv_get_state_len(); or NULL, if this is a first pass.</param>
        /// <param name="state_out">FP32 buffer of size rwkv_get_state_len(). This buffer will be written to if non-NULL.</param>
        /// <param name="logits_out">FP32 buffer of size rwkv_get_logits_len(). This buffer will be written to if non-NULL.</param>
        /// <returns></returns>
#if NET7_0_OR_GREATER
        [LibraryImport(LIBRARY_NAME)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        [return: MarshalAs(UnmanagedType.I1)]
        public static partial bool rwkv_eval(IntPtr ctx, uint token, IntPtr state_in, IntPtr state_out, IntPtr logits_out);
#else
        [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool rwkv_eval(IntPtr ctx, uint token, IntPtr state_in, IntPtr state_out, IntPtr logits_out);
#endif

        /// <summary>
        /// Evaluates the model for a sequence of tokens.
        /// Uses a faster algorithm than `rwkv_eval` if you do not need the state and logits for every token. Best used with sequence lengths of 64 or so.
        /// Has to build a computation graph on the first call for a given sequence, but will use this cached graph for subsequent calls of the same sequence length.
        ///
        /// NOTE ON GGML NODE LIMIT
        ///
        /// ggml has a hard-coded limit on max amount of nodes in a computation graph. The sequence graph is built in a way that quickly exceedes
        /// this limit when using large models and/or large sequence lengths.
        /// Fortunately, rwkv.cpp's fork of ggml has increased limit which was tested to work for sequence lengths up to 64 for 14B models.
        ///
        /// If you get `GGML_ASSERT: ...\ggml.c:16941: cgraph->n_nodes < GGML_MAX_NODES`, this means you've exceeded the limit.
        /// To get rid of the assertion failure, reduce the model size and/or sequence length.
        ///
        /// TODO When Metal (MPS) support is implemented, check that large sequence lengths work
        ///
        /// You can pass NULL to logits_out whenever logits are not needed. This can improve speed by ~10 ms per iteration, because logits are not calculated.
        /// Not thread-safe. For parallel inference, call `rwkv_clone_context` to create one rwkv_context for each thread.
        /// Returns false on any error.
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="tokens">pointer to an array of tokens. If NULL, the graph will be built and cached, but not executed: this can be useful for initialization.</param>
        /// <param name="sequence_len">number of tokens to read from the array.</param>
        /// <param name="state_in">FP32 buffer of size rwkv_get_state_len(), or NULL if this is a first pass.</param>
        /// <param name="state_out">FP32 buffer of size rwkv_get_state_len(). This buffer will be written to if non-NULL.</param>
        /// <param name="logits_out">FP32 buffer of size rwkv_get_logits_len(). This buffer will be written to if non-NULL.</param>
        /// <returns></returns>
#if NET7_0_OR_GREATER
        [LibraryImport(LIBRARY_NAME)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        [return: MarshalAs(UnmanagedType.I1)]
        public static partial bool rwkv_eval_sequence(IntPtr ctx, uint[] tokens, ulong sequence_len, IntPtr state_in, IntPtr state_out, IntPtr logits_out);
#else
        [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool rwkv_eval_sequence(IntPtr ctx, uint[] tokens, ulong sequence_len, IntPtr state_in, IntPtr state_out, IntPtr logits_out);
#endif

        /// <summary>
        /// Evaluates the model for a sequence of tokens using `rwkv_eval_sequence`, splitting a potentially long sequence into fixed-length chunks.
        /// This function is useful for processing complete prompts and user input in chat & role-playing use-cases.
        /// It is recommended to use this function instead of `rwkv_eval_sequence` to avoid mistakes and get maximum performance.
        ///
        /// Chunking allows processing sequences of thousands of tokens, while not reaching the ggml's node limit and not consuming too much memory.
        /// A reasonable and recommended value of chunk size is 16. If you want maximum performance, try different chunk sizes in range [2..64]
        /// and choose one that works the best in your use case.
        ///
        /// Not thread-safe. For parallel inference, call `rwkv_clone_context` to create one rwkv_context for each thread.
        /// Returns false on any error.
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="tokens">pointer to an array of tokens. If NULL, the graph will be built and cached, but not executed: this can be useful for initialization.</param>
        /// <param name="sequence_len">number of tokens to read from the array.</param>
        /// <param name="chunk_size">size of each chunk in tokens, must be positive.</param>
        /// <param name="state_in">FP32 buffer of size rwkv_get_state_len(), or NULL if this is a first pass.</param>
        /// <param name="state_out">FP32 buffer of size rwkv_get_state_len(). This buffer will be written to if non-NULL.</param>
        /// <param name="logits_out">FP32 buffer of size rwkv_get_logits_len(). This buffer will be written to if non-NULL.</param>
        /// <returns></returns>
#if NET7_0_OR_GREATER
        [LibraryImport(LIBRARY_NAME)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        [return: MarshalAs(UnmanagedType.I1)]
        public static partial bool rwkv_eval_sequence_in_chunks(IntPtr ctx, uint[] tokens, ulong sequence_len, ulong chunk_size, IntPtr state_in, IntPtr state_out, IntPtr logits_out);
#else
        [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool rwkv_eval_sequence_in_chunks(IntPtr ctx, uint[] tokens, ulong sequence_len, ulong chunk_size, IntPtr state_in, IntPtr state_out, IntPtr logits_out);
#endif

        /// <summary>
        /// Returns the number of tokens in the given model's vocabulary.
        /// Useful for telling 20B_tokenizer models (n_vocab = 50277) apart from World models (n_vocab = 65536).
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
#if NET7_0_OR_GREATER
        [LibraryImport(LIBRARY_NAME)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial ulong rwkv_get_n_vocab(IntPtr ctx);
#else
        [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern ulong rwkv_get_n_vocab(IntPtr ctx);
#endif

        /// <summary>
        /// Returns the number of elements in the given model's embedding.
        /// Useful for reading individual fields of a model's hidden state.
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
#if NET7_0_OR_GREATER
        [LibraryImport(LIBRARY_NAME)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial ulong rwkv_get_n_embed(IntPtr ctx);
#else
        [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern ulong rwkv_get_n_embed(IntPtr ctx);
#endif

        /// <summary>
        /// Returns the number of layers in the given model.
        /// A layer is a pair of RWKV and FFN operations, stacked multiple times throughout the model.
        /// Embedding matrix and model head (unembedding matrix) are NOT counted in `n_layer`.
        /// Useful for always offloading the entire model to GPU.
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
#if NET7_0_OR_GREATER
        [LibraryImport(LIBRARY_NAME)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial ulong rwkv_get_n_layer(IntPtr ctx);
#else
        [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern ulong rwkv_get_n_layer(IntPtr ctx);
#endif

        /// <summary>
        /// Returns the number of float elements in a complete state for the given model.
        /// This is the number of elements you'll need to allocate for a call to rwkv_eval, rwkv_eval_sequence, or rwkv_init_state.
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
#if NET7_0_OR_GREATER
        [LibraryImport(LIBRARY_NAME)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial ulong rwkv_get_state_len(IntPtr ctx);
#else
        [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern ulong rwkv_get_state_len(IntPtr ctx);
#endif

        /// <summary>
        /// Returns the number of float elements in the logits output of a given model.
        /// This is currently always identical to n_vocab.
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
#if NET7_0_OR_GREATER
        [LibraryImport(LIBRARY_NAME)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial ulong rwkv_get_logits_len(IntPtr ctx);
#else
        [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern ulong rwkv_get_logits_len(IntPtr ctx);
#endif

        /// <summary>
        /// Initializes the given state so that passing it to rwkv_eval or rwkv_eval_sequence would be identical to passing NULL.
        /// Useful in cases where tracking the first call to these functions may be annoying or expensive.
        /// State must be initialized for behavior to be defined, passing a zeroed state to rwkv.cpp functions will result in NaNs.
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="state">FP32 buffer of size rwkv_get_state_len() to initialize</param>
#if NET7_0_OR_GREATER
        [LibraryImport(LIBRARY_NAME)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial void rwkv_init_state(IntPtr ctx, float[] state);
#else
        [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern void rwkv_init_state(IntPtr ctx, float[] state);
#endif

        /// <summary>
        /// Frees all allocated memory and the context.
        /// Does not need to be called on the same thread that created the rwkv_context.
        /// </summary>
        /// <param name="ctx"></param>
#if NET7_0_OR_GREATER
        [LibraryImport(LIBRARY_NAME)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial void rwkv_free(IntPtr ctx);
#else
        [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern void rwkv_free(IntPtr ctx);
#endif

        /// <summary>
        /// Quantizes FP32 or FP16 model to one of quantized formats.
        /// Returns false on any error. Error messages would be printed to stderr.
        /// </summary>
        /// <param name="model_file_path_in">path to model file in ggml format, must be either FP32 or FP16.</param>
        /// <param name="model_file_path_out">quantized model will be written here.</param>
        /// <param name="format_name">
        /// must be one of available format names below.
        /// Available format names:
        /// - Q4_0
        /// - Q4_1
        /// - Q5_0
        /// - Q5_1
        /// - Q8_0
        /// </param>
        /// <returns></returns>
#if NET7_0_OR_GREATER
        [LibraryImport(LIBRARY_NAME, StringMarshalling = StringMarshalling.Utf8)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        [return: MarshalAs(UnmanagedType.I1)]
        public static partial bool rwkv_quantize_model_file(string model_file_path_in, string model_file_path_out, string format_name);
#else
        [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool rwkv_quantize_model_file(string model_file_path_in, string model_file_path_out, string format_name);
#endif

        /// <summary>
        /// Returns system information string.
        /// </summary>
        /// <returns></returns>
#if NET7_0_OR_GREATER
        [LibraryImport(LIBRARY_NAME, StringMarshalling = StringMarshalling.Utf8)]
        [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static partial string rwkv_get_system_info_string();
#else
        [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern string rwkv_get_system_info_string();
#endif
    }
}
