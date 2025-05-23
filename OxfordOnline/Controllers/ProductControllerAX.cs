using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc;
using OxfordOnline.Interfaces;
using OxfordOnline.Models;
using System.ServiceModel;
using Oxfordonline.Integration;
using OxfordOnline.Controllers;

//[ApiController]
//[Route("api/product")]
public class ProductControllerAX : ControllerBase
{
    /*
    static readonly ProductInterface repositorio = new ProductAX();
    TokenKey tokenKey = new TokenKey();

    [HttpGet("all")]
    public ActionResult<IEnumerable<ProdutosAX>> GetAllProdutos()
    {
        try
        {
            return Ok(repositorio.GetAll()); // Retorna 200 OK com a lista de produtos
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro ao chamar o serviço", error = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public IActionResult GetProduto(string id)
    {
        try
        {
            ProdutosAX item = repositorio.Get(id);
            if (item == null)
            {
                return NotFound();
            }
            return Ok(item);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro ao chamar o serviço", error = ex.Message});
        }
    }

    [HttpGet("find/{itemId}")]
    public async Task<IActionResult> FindProduct(string itemId)
    {
        try
        {
            CallContext AxDocumentContext = new CallContext();
            AxDocumentContext.Company = "100";
            // Cria o cliente do serviço WCF
            var wsClient = new ProductServicesClient(new BasicHttpBinding(),
                new EndpointAddress("http://ax201203:8201/DynamicsAx/Services/WSIntegratorServices"));

            var callContext = new CallContext { Company = "100" };

            // Cria request e chama serviço do Dynamics AX
            //var request = new ProductServicesFindEstruturaProdutoRequest(callContext, itemId);
            var response = await wsClient.findAsync(AxDocumentContext, itemId);

            return Ok(new { message = "Sucesso", data = response });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro ao chamar o serviço", error = ex.Message });
        }
    }*/
}
/*
 namespace OxfordOnline.Controllers
{
    [ApiController]
[Route("api/product")]
    public class ProductController : ControllerBase
    {
        static readonly ProductInterface repositorio = new Product();

        TokenKey tokenKey = new TokenKey();
        public IEnumerable<Produtos> GetAllProdutos()
        {
            return repositorio.GetAll();
        }

        public IActionResult GetProduto(string id)
        {
            Produtos item = repositorio.Get(id);
            if (item == null)
            {
                return NotFound();
            }
            return Ok(item);
        }
    }
}
*/