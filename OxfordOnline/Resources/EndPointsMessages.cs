using static System.Runtime.InteropServices.JavaScript.JSType;

namespace OxfordOnline.Resources
{
    public class EndPointsMessages
    {
        public const string TokenMissingOrInvalid    = "Token ausente ou inválido.";
        public const string TokenInvalid             = "Token inválido.";
        public const string TokenFormatInvalid       = "Insira o token no formato: Bearer {seu_token}";
        public const string UserAndPasswordRequired  = "Usuário e senha obrigatórios.";
        public const string EmailRequired            = "Email não encontrado.";
        public const string UserAlreadyRegistered    = "Usuário já cadastrado.";
        public const string UserRegisteredSuccess    = "Usuário cadastrado com sucesso.";
        public const string InvalidUserOrPassword    = "Usuário ou senha inválidos.";
        public const string LogErrorRegisterUser     = "Erro ao registrar usuário.";
        public const string LogErrorLoginUser        = "Erro ao realizar login do usuário.";
                                                     
        public const string ProductNotFound          = "Produto não encontrado.";
        public const string InvalidProductList       = "Lista de produtos inválida ou vazia.";
        public const string ErrorSavingProducts      = "Erro ao salvar produtos.";
        public const string InvalidProductData       = "Dados do produto inválidos ou ID não corresponde.";
        public const string ProductNotFoundForUpdate = "Produto não encontrado para atualização.";
        public const string ErrorUpdatingProduct     = "Erro ao atualizar produto.";
        public const string ProductNotFoundForDelete = "Produto não encontrado para exclusão.";
        public const string ErrorDeletingProduct     = "Erro ao excluir produto.";
        public const string ProductDeletedSuccess    = "Produto excluído com sucesso.";
        public const string ProductSavedSuccess      = "{0} produto(s) inserido(s) ou atualizado(s) com sucesso.";
                                                     
        public const string LogErrorSavingProduct    = "Erro ao salvar o produto.";
        public const string LogErrorUpdatingProduct  = "Erro ao atualizar produto com id {ProductId}.";
        public const string LogErrorDeletingProduct  = "Erro ao excluir produto com id {ProductId}.";

        // Imagens
        public const string NoImagesProvided         = "Nenhuma imagem foi enviada.";
        public const string InvalidImageField        = "Todas as imagens devem ter um ProductId e um caminho da imagem.";
        public const string ImageSavedSuccess        = "{0} imagem(ns) adicionada(s) com sucesso!";
        public const string ImageNotFoundForProduct  = "Nenhuma imagem encontrada para o produto informado.";
        public const string ImageNotFound            = "Imagem não encontrada.";
        public const string ErrorSavingImages        = "Erro ao salvar as imagens.";
        public const string LogErrorSavingImage      = "Erro ao salvar imagens.";

        // FTP
        public const string NoPath = "Diretório não encontrado ou não informado.";
        public const string ErrorFTP = "Erro FTP: %Error%";
        public const string ImageNull = "Nenhuma imagem encontrada.";
        public const string InvalidFile = "Conteúdo inválido.";

        // Pallet
        public const string ImagesOnPalletNotOk = "Todas as imagens na lista devem pertencer ao mesmo PalletId.";

        // Default
        public const string Error = "Erro: %Error%";
        public const string Sucess = "Concluído com sucesso.";
        public const string DeleteOk = "Excluído com sucesso.";
        public const string ErrorDel = "Erro ao excluir";
        public const string ErrorUp = "Erro ao atualizar";
        public const string ErrorSave = "Erro ao salvar";
        public const string SucessSave = "Salvo com sucesso.";
        public const string UpdateOk = "Atualizado com sucesso.";
        public const string NotFound = "Não encontrado.";

    }
}
