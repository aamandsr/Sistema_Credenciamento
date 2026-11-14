namespace Sistema_Credenciamento.Models
{
    public class Congressista
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Nome_Cracha { get; set; }
        public string Tipo { get; set; } // Palestrante ou Congressista
        public string Pagamento { get; set; } // Sim, Não ou Cortesia
        public Boolean Material_Impresso { get; set; } // Sim, Não
        public string? Empresa { get; set; } // Aq deveria ser o id e fazer a relação com a tabela Empresa, 
        // mas como quero simplificar vai ser manualmente o nome da empresa, posteriormente posso fazer a relação com uma tabela Empresa
    }
}