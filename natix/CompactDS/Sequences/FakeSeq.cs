////
////  Copyright 2012  Eric Sadit Tellez Avila
////
////    Licensed under the Apache License, Version 2.0 (the "License");
////    you may not use this file except in compliance with the License.
////    You may obtain a copy of the License at
////
////        http://www.apache.org/licenses/LICENSE-2.0
////
////    Unless required by applicable law or agreed to in writing, software
////    distributed under the License is distributed on an "AS IS" BASIS,
////    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
////    See the License for the specific language governing permissions and
////    limitations under the License.
//using System;
//using System.IO;
//using System.Collections;
//using System.Collections.Generic;
//using natix.SortingSearching;
//
//namespace natix.CompactDS
//{
//	public class SeqPlain : IRankSelectSeq
//	{
//		IList<int> SEQ;
//		IRankSelect X;
//		int sigma;
//
//		public SeqPlain ()
//		{
//		}
//
//		public void Build (IList<int> seq, int sigma, ListIBuilder list_builder = null, BitmapFromBitStream bitmap_builder = null)
//		{
//			if (list_builder == null) {
//				list_builder = ListIBuilders.GetListIFS ();
//			}
//			if (bitmap_builder == null) {
//				bitmap_builder = BitmapBuilders.GetGGMN_wt(16);
//			}
//			this.sigma = sigma;
//			var S = new BitStream32[sigma];
//			int n = seq.Count;
//			int s = 0;
//			for (int i = 0; i < n; ++i) {
//				if (i % sigma == 0) {
//					for (int c = 0; c < sigma; ++c) {
//						if (i == 0) {
//							S[c] = new BitStream32();
//						}
//						S[c].Write(true);
//					}
//					s++;
//				}
//				S[seq[i]].Write (false);
//			}
//			var ostream = S[0];
//			for (int c = 1; c < sigma; ++c) {
//				var istream = S[c];
//				for (int i = 0; i < istream.CountBits; ++i) {
//					ostream.Write(istream[i]);
//				}
//			}
//			// Console.WriteLine ("OSTREAM: {0}", ostream);
//			this.X = bitmap_builder(new FakeBitmap(ostream));
//			this.SEQ = list_builder(seq, sigma);
//		}
//
//		public void Load(BinaryReader Input)
//		{
//			this.SEQ = ListIGenericIO.Load(Input);
//			this.X = RankSelectGenericIO.Load(Input);
//			this.sigma = Input.ReadInt32 ();
//		}
//
//		public void Save(BinaryWriter Output)
//		{
//			ListIGenericIO.Save(Output, this.SEQ);
//			RankSelectGenericIO.Save(Output, this.X);
//			Output.Write ((int)this.sigma);
//		}
//
//		public int Sigma {
//			get {
//				return sigma;
//			}
//		}
//
//		public int Count {
//			get {
//				return this.SEQ.Count;
//			}
//		}
//
//		public IRankSelect Unravel(int sym)
//		{
//			return new UnraveledSymbol(this, sym);
//		}
//
//		public int Access(int pos)
//		{
//			return this.SEQ[pos];
//		}
//
//		public int PopCount (int symbol, int sp, int ep)
//		{
//			int pc = 0;
//			while (sp <= ep) {
//				if (this.SEQ[sp] == symbol) {
//					++pc;
//				}
//				++sp;
//			}
//			return pc;
//		}
//
//		public int FindPopCount (int symbol, int sp, int count)
//		{
//			var n = this.SEQ.Count;
//			var pc = 0;
//			var i = 0;
//			while (pc < count && sp < n) {
//				if (this.SEQ [sp] == symbol) {
//					++pc;
//				}
//				++sp;
//				++i;
//			}
//			if (pc != count) {
//				throw new IndexOutOfRangeException(String.Format ("symbol: {0}, sp: {1}, count: {2}, pc: {3}, i: {4}",
//				                                                  symbol, sp, count, pc, i));
//			}
//			return sp - 1;
//		}
//
//		public int Rank (int symbol, int pos)
//		{
//			if (pos < 0) {
//				return 0;
//			}
//			//Console.WriteLine ("XXX symbol: {0}, pos: {1}, pos/sigma: {2}", symbol, pos, pos / this.Sigma);
//			int m = this.Count / this.Sigma;
//			var a_rank1 = m * symbol + pos / this.Sigma + 1;
//			var a_pos = this.X.Select1(a_rank1);
//			var a_rank0 = a_pos + 1 - a_rank1;
//			//Console.WriteLine ("m: {0}, a_rank0: {1}, a_rank1: {2}, a_pos: {3}", m, a_rank0, a_rank1, a_pos);
//			// apos + 1 = arank1 + arank0
//			var b_rank1 = m * symbol + 1;
//			var b_pos = this.X.Select1(b_rank1);
//			var b_rank0 = b_pos + 1 - b_rank1;
//			//Console.WriteLine ("m: {0}, b_rank0: {1}, b_rank1: {2}, b_pos: {3}", m, b_rank0, b_rank1, b_pos);
//			var rem = pos % this.Sigma;
//			var pc = this.PopCount(symbol, pos - rem,  pos);
//			var r = a_rank0 - b_rank0 + pc;
//			//Console.WriteLine ("symbol: {0}, pos: {1}, r: {2}, pc: {3}", symbol, pos, r, pc);
//			return r;
//		}
//
//		public int Select (int symbol, int rank)
//		{
//			if (rank <= 0) {
//				return -1;
//			}
//			// locating the position on X of symbol
//			var n = this.Count;
//			int m = n / this.Sigma;
//			var a_rank1 = m * symbol + 1;
//			var a_pos = this.X.Select1 (a_rank1);
//			var a_rank0 = a_pos + 1 - a_rank1;
//
//			// locating the position on X of rank
//			var b_rank0 = a_rank0 + rank;
//			var b_pos = this.X.Select0 (b_rank0);
//			var b_rank1 = b_pos + 1 - b_rank0;
//
//			// determining the block Id containing the result
//			var blockId = (b_rank1 - 1) % m;
//
//			// determining the remainder rank' on the block
//			var c_rank1 = b_rank1;
//			var c_pos = this.X.Select1 (c_rank1);
//			var c_rank0 = c_pos + 1 - c_rank1;
//			var rank_relative = b_rank0 - c_rank0;
//
////			try {
//				// last sequential search of the relative rank starting on the container block
//				return this.FindPopCount (symbol, blockId * this.Sigma, rank_relative);
////			} catch (Exception e) {
////				var N = this.X.Count;
////				Console.WriteLine ("=== select {0}, rank: {1}, num-symbols: {2}, n: {3}, X.Count = {4}", symbol, rank, this.Rank (symbol, n-1), n, N);
////				Console.WriteLine ("sel m: {0}, a_rank0: {1}, a_rank1: {2}, a_pos: {3}", m, a_rank0, a_rank1, a_pos);
////				Console.WriteLine ("sel m: {0}, b_rank0: {1}, b_rank1: {2}, b_pos: {3}", m, b_rank0, b_rank1, b_pos);
////				Console.WriteLine ("sel m: {0}, c_rank0: {1}, c_rank1: {2}, c_pos: {3}", m, c_rank0, c_rank1, c_pos);
////				Console.WriteLine ("sel blockId: {0}, b_rank0 - c_rank0: {1}", blockId, rank_relative);
////				throw e;
////			}
//		}
//	}
//}
//
