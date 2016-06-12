using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using FieldGenerator.Models;

namespace FieldGenerator.Controllers
{
    public class FieldGeneratorController : ApiController
    {

        int[,] board = new int[4, 4];

        public IHttpActionResult GetField()
        {
            board = Generator.Generate();
            return Ok(board);
        }
    }
}
