public static class RegisterLocationClothesEndpoint
{
    public static void MapRegisterLocationWorkGuide(this IEndpointRouteBuilder app)
    {
        app.MapPost("/RegisterWorkGuide", async (RecepcionDbContext context, CreateLocationWorkGuideRequestDto request, IAppLogger<string> logger) =>
        {
            if (request.Guias == null || request.Guias.Count() == 0)
            {
                logger.LogInformacion("NumeroGuia esta vacio", "RegisterLocationClothesEndpoint");
                var responseVal = new ApiResponse<string>
                {
                    Data = "",
                    Message = "NumeroGuia esta vacio",
                    StatusCode = StatusCodes.Status400BadRequest,
                    Success = false
                };
                return Results.BadRequest(responseVal);
            }

            List<string> errores = [];


            foreach (var item in request.Guias)
            {

                var numeroGuia = item.NumeroGuia.Substring(0, item.NumeroGuia.Length - 1);
                var letraGuia = item.NumeroGuia.Substring(item.NumeroGuia.Length - 1);
                var isSystem = item.IsSystem;

                if (isSystem)
                {



                    if (string.IsNullOrEmpty(numeroGuia) || string.IsNullOrEmpty(letraGuia))
                    {
                        logger.LogInformacion("NumeroGuia ó LetraGuia esta vacio", "RegisterLocationClothesEndpoint");
                        errores.Add($"NumeroGuia ó LetraGuia esta vacio {numeroGuia} {letraGuia}");
                        continue;
                    }

                    var workGuideId = context.WorkGuideMains
                        .Where(wg => wg.NumeroGuia == numeroGuia)
                        .Select(wg => wg.Id)
                        .FirstOrDefault();

                    if (workGuideId == 0)
                    {
                        logger.LogInformacion("Numero de Guia Cabecera no encontrado", "RegisterLocationClothesEndpoint");
                        errores.Add($"Numero de Guia Cabecera no encontrado {numeroGuia}");
                        continue;
                    }

                    var workGuideDetailId = context.WorkGuideDetails
                        .Where(wgd => wgd.WorkGuideMainId == workGuideId && wgd.Identificador == letraGuia)
                        .FirstOrDefault();

                    if (workGuideDetailId == null)
                    {
                        logger.LogInformacion("Detalle de guia no encontrado", "RegisterLocationClothesEndpoint");
                        errores.Add($"Detalle de guia no encontrado {letraGuia}");
                        continue;
                    }


                    var location = new LocationWorkGuide
                    {
                        LocationClothesId = request.LocationClothesId,
                        WorkGuideId = workGuideId,
                        WorkGuideDetailId = workGuideDetailId.Id,
                        Comments = request.Comments,
                        NumeroGuia = item.NumeroGuia,
                        IsSystem = isSystem,
                        CreatedDate = DateTime.Now
                    };

                    var ubicacion = context.LocationClothes
                        .Where(lc => lc.Id == request.LocationClothesId)
                        .Select(lc => lc.Name)
                        .FirstOrDefault();

                    if (ubicacion == null)
                    {
                        logger.LogInformacion("Ubicacion maestro no encontrado", "RegisterLocationClothesEndpoint");
                        errores.Add($"Ubicacion maestro no encontrado {request.LocationClothesId}");
                        continue;
                    }

                    workGuideDetailId.Ubicacion = ubicacion;
                    context.LocationWorkGuides.Add(location);
                    await context.SaveChangesAsync();

                }else{
                    
                    var location = new LocationWorkGuide
                    {
                        LocationClothesId = request.LocationClothesId,
                        WorkGuideId = null,
                        WorkGuideDetailId = null,
                        Comments = item.Referencia,
                        NumeroGuia = item.NumeroGuia,
                        IsSystem = isSystem,
                        CreatedDate = DateTime.Now
                    };

                    context.LocationWorkGuides.Add(location);
                    await context.SaveChangesAsync();
                }

            }

            var response = new ApiResponse<string>
            {
                Data = "",
                Message = "Ubicacion registrada",
                StatusCode = StatusCodes.Status201Created,
                Success = true,
                Errors = errores.ToList()
            };

            return Results.Ok(response);


        });
    }
}