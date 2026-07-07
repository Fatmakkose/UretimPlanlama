using System;
using System.Collections.Generic;

namespace UretimPlanlama.Models
{
    public class CariHareketRequest
    {
        public int CariHesapId { get; set; }
        public DateTime IslemTarihi { get; set; }
        public string IslemTipi { get; set; } = "Borç";
        public string? Aciklama { get; set; }
        public decimal Tutar { get; set; }
        public int? OrderId { get; set; }
        public string? BelgeNo { get; set; }

        public List<StokKalemDto>? StokKalemleri { get; set; }
    }

    public class StokKalemDto
    {
        public int StokKartiId { get; set; }
        public int? StokVaryantId { get; set; }
        public decimal Miktar { get; set; }
        public decimal? BirimFiyat { get; set; }
        public string? VaryantAdi { get; set; }
    }
}
