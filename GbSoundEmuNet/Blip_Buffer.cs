// Buffer of sound samples into which band-limited waveforms can be synthesized
// using Blip_Wave or Blip_Synth.

// Blip_Buffer 0.3.4. Copyright (C) 2003-2005 Shay Green. GNU LGPL license.

// Copyright (C) 2003-2005 Shay Green. This module is free software; you
// can redistribute it and/or modify it under the terms of the GNU Lesser
// General Public License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version. This
// module is distributed in the hope that it will be useful, but WITHOUT ANY
// WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
// FOR A PARTICULAR PURPOSE. See the GNU Lesser General Public License for
// more details. You should have received a copy of the GNU Lesser General
// Public License along with this module; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA

// Ported to C# August 2019 Adrian Conlon

namespace GbSoundEmuNet
{
    using System;
    using System.Collections.Generic;

    public class Blip_Buffer
    {
        // Make buffer as large as possible (currently about 65000 samples)
        public const int blip_default_length = 0;

        public const int BLIP_BUFFER_ACCURACY = 16;

        // Construct an empty buffer.
        public Blip_Buffer()
        {
            samples_per_sec = 44100;

            // try to cause assertion failure if buffer is used before these are set
            clocks_per_sec = 0;
            factor_ = ~0ul;
            offset_ = 0;
            buffer_size_ = 0;
            length_ = 0;

            bass_freq_ = 16;
        }

        // Set output sample rate and buffer length in milliseconds (1/1000 sec),
        // then clear buffer. If length is not specified, make as large as possible.
        // If there is insufficient memory for the buffer, sets the buffer length
        // to 0 and returns error string (or propagates exception if compiler supports it).
        public void set_sample_rate(long new_rate, int msec = blip_default_length)
        {
            uint new_size = (uint.MaxValue >> BLIP_BUFFER_ACCURACY) + 1 - widest_impulse_ - 64;
            if (msec != blip_default_length)
            {
                uint s = (uint)(((new_rate * (msec + 1)) + 999) / 1000);
                if (s < new_size)
                    new_size = s;
                else
                    throw new InvalidOperationException("requested buffer length exceeds limit");
            }

            if (buffer_size_ != new_size)
            {
                buffer_size_ = 0;
                offset_ = 0;

                const int count_clocks_extra = 2;
                Array.Resize(ref buffer_, (int)(new_size + widest_impulse_ + count_clocks_extra));
            }

            buffer_size_ = new_size;
            length_ = (int)((new_size * 1000 / new_rate) - 1);
            if (msec != 0)
                System.Diagnostics.Debug.Assert(length_ == msec); // ensure length is same as that passed in

            samples_per_sec = new_rate;
            if (clocks_per_sec != 0)
                clock_rate(clocks_per_sec); // recalculate factor

            bass_freq(bass_freq_); // recalculate shift

            clear();
        }

        // Length of buffer, in milliseconds
        public int length()
        {
            return length_;
        }

        // Current output sample rate
        public long sample_rate()
        {
            return samples_per_sec;
        }

        // Number of source time units per second
        public void clock_rate(long cps)
        {
            clocks_per_sec = cps;
            factor_ = clock_rate_factor(cps);
        }

        public long clock_rate() {
            	return clocks_per_sec;
        }

        // Set frequency at which high-pass filter attenuation passes -3dB
        public void bass_freq(int freq)
        {
            bass_freq_ = freq;
            if (freq == 0)
            {
                bass_shift = 31; // 32 or greater invokes undefined behavior elsewhere
                return;
            }
            bass_shift = 1 + (int)Math.Floor(1.442695041 * Math.Log(0.124 * samples_per_sec / freq));
            if (bass_shift < 0)
                bass_shift = 0;
            if (bass_shift > 24)
                bass_shift = 24;
        }

        // Remove all available samples and clear buffer to silence. If 'entire_buffer' is
        // false, just clear out any samples waiting rather than the entire buffer.
        public void clear(bool entire_buffer = true)
        {
            long count = (entire_buffer ? buffer_size_ : samples_avail());
            offset_ = 0;
            reader_accum = 0;
            if (buffer_.Length != 0)
            {
                for (int i = 0; i < count + widest_impulse_; ++i)
                    buffer_[i] = sample_offset_;
            }
        }

        // End current time frame of specified duration and make its samples available
        // (along with any still-unread samples) for reading with read_samples(). Begin
        // a new time frame at the end of the current frame. All transitions must have
        // been added before 'time'.
        public void end_frame(long t)
        {
            offset_ += (ulong)t * this.factor_;
            //assert(("Blip_Buffer::end_frame(): Frame went past end of buffer",
            //    samples_avail() <= (long)buffer_size_));
        }

        // Number of samples available for reading with read_samples()
        public long samples_avail()
        {
            return (long)(offset_ >> BLIP_BUFFER_ACCURACY);
        }

        // Read at most 'max_samples' out of buffer into 'dest', removing them from from
        // the buffer. Return number of samples actually read and removed. If stereo is
        // true, increment 'dest' one extra time after writing each sample, to allow
        // easy interleving of two channels into a stereo output buffer.
        //public long read_samples(short[] buf, int dest, long max_samples, bool stereo = false)
        //{
        //    long count = samples_avail();
        //    if (count > max_samples)
        //        count = max_samples;

        //    if (count == 0)
        //        return 0; // optimization

        //    int sample_offset_ = Blip_Buffer.sample_offset_;
        //    int bass_shift = this.bass_shift;
        //    long accum = reader_accum;

        //    //            if (!stereo)
        //    //            {
        //    for (long n = count; (n--) != 0;)
        //    {
        //        long s = accum >> accum_fract;
        //        accum -= accum >> bass_shift;
        //        accum += ((*buf++) - sample_offset_) << accum_fract;
        //        *dest++ = (short)s;

        //        // clamp sample
        //        if ((int16_t)s != s)
        //            dest[-1] = blip_sample_t(0x7FFF - (s >> 24));
        //    }
        //    //}
        //    //	else
        //    //	{
        //    //		for (long n = count; n--; )
        //    //		{
        //    //			long s = accum >> accum_fract;
        //    //accum -= accum >> bass_shift;
        //    //			accum += (long (* buf++) - sample_offset_) << accum_fract;

        //    //            *out = (blip_sample_t) s;
        //    //			out += 2;

        //    //			// clamp sample
        //    //			if ((int16_t) s != s )
        //    //				out [-2] = blip_sample_t(0x7FFF - (s >> 24));
        //    //		}
        //    //	}

        //    //	reader_accum = accum;

        //    //	remove_samples(count );

        //    //	return count;

        //}

        //      // Remove 'count' samples from those waiting to be read
        //      public void remove_samples(long count);

        //      // Number of samples delay from synthesis to samples read out
        //      public int output_latency() const;

        //      // Beta features

        //      // Number of raw samples that can be mixed within frame of specified duration
        //      public long count_samples(blip_time_t duration) const;

        //      // Mix 'count' samples from 'buf' into buffer.
        //      public void mix_samples( const blip_sample_t* buf, long count );

        //      // Count number of clocks needed until 'count' samples will be available.
        //      // If buffer can't even hold 'count' samples, returns number of clocks until
        //      // buffer is full.
        //      public blip_time_t count_clocks(long count) const;


        //      // not documented yet

        //      public void remove_silence(long count);

        //      public ulong resampled_time(blip_time_t t) const
        //	    {
        //    return t* ulong(factor_) + offset_;
        //   }

        public ulong clock_rate_factor(long clock_rate)
        {
            ulong factor = (ulong) Math.Floor(
                    (double)samples_per_sec / clock_rate * (1L << BLIP_BUFFER_ACCURACY) + 0.5);
            if (factor <= 0)
                throw new InvalidOperationException("clock_rate/sample_rate ratio is too large");
            return factor;
        }

        public ulong resampled_duration(int t)
        {
            return (ulong)t* factor_;
        }

        // Don't use the following members. They are public only for technical reasons.
        public const int sample_offset_ = 0x7F7F; // repeated byte allows memset to clear buffer
        public const int widest_impulse_ = 24;

        public ulong factor_;
        public ulong offset_;
        public ushort[] buffer_;
        public uint buffer_size_;

        private long reader_accum;
        private int bass_shift;
        private long samples_per_sec;
        private long clocks_per_sec;
        private int bass_freq_;
        private int length_;

        private const int accum_fract = 15; // less than 16 to give extra sample range
    }

    //  // Low-pass equalization parameters (see notes.txt)
    //  class blip_eq_t
    //  {
    //   public blip_eq_t(double treble = 0);
    //      public blip_eq_t(double treble, long cutoff, long sample_rate);

    //      private double treble;
    //      private long cutoff;
    //      private long sample_rate;
    //      private friend class Blip_Impulse_;
    //  };

    //  // not documented yet (see Multi_Buffer.cpp for an example of use)
    //  class Blip_Reader
    //  {
    //      private std::vector<Blip_Buffer::ushort>::const_iterator buf;
    //      private long accum;

    //      // avoid anything which might cause optimizer to put object in memory

    //      public int begin(Blip_Buffer& blip_buf )
    //      {
    //          buf = blip_buf.buffer_.cbegin();
    //          accum = blip_buf.reader_accum;
    //          return blip_buf.bass_shift;
    //      }

    //      public int read() const
    //      {
    //    return accum >> Blip_Buffer::accum_fract;
    //   }

    //      public void next(int bass_shift = 9)
    //      {
    //          accum -= accum >> bass_shift;
    //          accum += ((long)*buf++ - Blip_Buffer::sample_offset_) << Blip_Buffer::accum_fract;
    //      }

    //      public void end(Blip_Buffer& blip_buf )
    //      {
    //          blip_buf.reader_accum = accum;
    //      }
    //  }

    //  // End of public interface

    //  #ifndef BLIP_BUFFER_ACCURACY
    //   #define BLIP_BUFFER_ACCURACY 16
    //  #endif

    //  const int blip_res_bits_ = 5;

    //  typedef uint32_t blip_pair_t_;

    //  class Blip_Impulse_
    //  {
    //      typedef uint16_t imp_t;

    //   blip_eq_t eq;
    //      double volume_unit_;
    //      imp_t* impulses;
    //      imp_t* impulse;
    //      int width;
    //      int fine_bits;
    //      int res;
    //      bool generate;

    //      void fine_volume_unit();
    //      void scale_impulse(int unit, imp_t* ) const;
    //      public:
    //   Blip_Buffer* buf;
    //      uint32_t offset;

    //      void init(blip_pair_t_* impulses, int width, int res, int fine_bits = 0);
    //      void volume_unit(double );
    //      void treble_eq( const blip_eq_t& );
    //  }
}
