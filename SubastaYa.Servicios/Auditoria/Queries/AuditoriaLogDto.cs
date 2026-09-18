namespace SubastaYa.Servicios.Auditoria.Queries
{
    public class AuditoriaLogDto
    {
        public int Id { get; set; }
        public string Entidad { get; set; } = string.Empty;
        public int? Entidad_Id { get; set; }
        public string Accion { get; set; } = string.Empty;
        public int? Usuario_Id { get; set; }
        public string? Detalle_Json { get; set; }
        public DateTime Fecha { get; set; }
    }
}