namespace SistemaCredenciamento.Models
{
    // Representa os dados de inscrição usados pelas telas e pela tabela congressista.
    public class Congressista
    {
        // Identificador gerado pelo banco de dados.
        public int Id { get; set; }

        // Nome completo para consulta e identificação administrativa.
        public string Nome { get; set; }

        // Texto personalizado que será usado na etiqueta.
        public string Nome_Cracha { get; set; }

        // Categoria aceita atualmente: Congressista ou Palestrante.
        public string Tipo { get; set; }

        // Situação de pagamento: Sim, Não ou Cortesia.
        public string Pagamento { get; set; }

        // Indica se a etiqueta deste congressista já foi registrada como impressa.
        public bool Material_Impresso { get; set; }

        // Nome livre da empresa; não há relacionamento com uma tabela separada.
        public string? Empresa { get; set; }
    }

}