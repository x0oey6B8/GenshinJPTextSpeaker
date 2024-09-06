using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenshinJPTextSpeaker
{
    public class Class1
    {
        public Supported_Features supported_features { get; set; }
        public string name { get; set; }
        public string speaker_uuid { get; set; }
        public Style[] styles { get; set; }
        public string version { get; set; }
    }

    public class Supported_Features
    {
        public string permitted_synthesis_morphing { get; set; }
    }

    public class Style
    {
        public string name { get; set; }
        public int id { get; set; }
        public string type { get; set; }

        public override string ToString()
        {
            return $"[{id}]{name}";
        }
    }

    public class AudioQuery
    {
        public Accent_Phrases[] accent_phrases { get; set; }
        public double? speedScale { get; set; }
        public double? pitchScale { get; set; }
        public double? intonationScale { get; set; }
        public double? volumeScale { get; set; }
        public double? prePhonemeLength { get; set; }
        public double? postPhonemeLength { get; set; }
        public double? outputSamplingRate { get; set; }
        public bool outputStereo { get; set; }
        public string kana { get; set; }
    }

    public class Accent_Phrases
    {
        public Mora[] moras { get; set; }
        public double? accent { get; set; }
        public Pause_Mora pause_mora { get; set; }
        public bool is_interrogative { get; set; }
    }

    public class Pause_Mora
    {
        public string text { get; set; }
        public string consonant { get; set; }
        public double? consonant_length { get; set; }
        public string vowel { get; set; }
        public double? vowel_length { get; set; }
        public double? pitch { get; set; }
    }

    public class Mora
    {
        public string text { get; set; }
        public string consonant { get; set; }
        public double? consonant_length { get; set; }
        public string vowel { get; set; }
        public double? vowel_length { get; set; }
        public double? pitch { get; set; }
    }
}
