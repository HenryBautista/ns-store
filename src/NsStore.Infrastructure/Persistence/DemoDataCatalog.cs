namespace NsStore.Infrastructure.Persistence;

/// <summary>
/// The literal contents of the demo dataset: a laptop-parts store in Cochabamba, Bolivia.
/// Business-facing text is Spanish because it is what the client reads on screen; identifiers and
/// comments stay English, per the repository convention.
/// </summary>
/// <remarks>
/// Kept apart from <see cref="DemoDataSeeder"/> so the seeding <em>mechanics</em> (invariants,
/// ordering, transactions) can be read without wading through two hundred rows of catalog.
/// Costs are in BOB and reflect the local market; sale prices are never listed here — the seeder
/// derives them from the configured margin and VAT, the same way the pricing module does.
/// </remarks>
internal static class DemoDataCatalog
{
    public const string SecondBranchCode = "NORTE";

    /// <summary>Shared password for every demo seller. Only ever reachable behind the demo flag.</summary>
    public const string SellerPassword = "Demo1234";

    public static readonly (string Code, string Name, string Address, string Phone) SecondBranch =
        (SecondBranchCode, "Sucursal Norte", "Av. América E-350, entre Pando y Beijing, Cochabamba", "+591 4 4238877");

    public static readonly (string Username, string FirstName, string LastName, string MotherLastName, string BranchCode)[] Sellers =
    [
        ("mquispe", "Marisol", "Quispe", "Ticona", "MAIN"),
        ("jvargas", "Jhonny", "Vargas", "Mamani", "MAIN"),
        ("dcamacho", "Daniela", "Camacho", "Rojas", SecondBranchCode)
    ];

    public static readonly string[] Trademarks =
    [
        "HP", "Dell", "Lenovo", "Acer", "Asus", "Toshiba", "Samsung", "LG",
        "Kingston", "ADATA", "Western Digital", "Seagate", "Crucial", "Genérico"
    ];

    public static readonly string[] Categories =
    [
        "Pantallas LED", "Teclados", "Baterías", "Cargadores", "Memorias RAM", "Discos SSD",
        "Discos duros HDD", "Bisagras", "Carcasas", "Ventiladores y coolers", "Placas madre",
        "Flex de video", "Touchpads", "Parlantes", "Cámaras web", "Lectoras DVD",
        "Cables y adaptadores", "Accesorios"
    ];

    /// <summary>Free text in uppercase, as the legacy system stored it and as the SPA parses it.</summary>
    public static readonly string[] WarrantyTerms = ["SIN GARANTÍA", "3 MESES", "6 MESES", "1 AÑO"];

    public static readonly (string Name, string Phone, string Email)[] Suppliers =
    [
        ("Importadora Andina S.R.L.", "+591 2 2451188", "ventas@importadoraandina.bo"),
        ("TecnoPartes Bolivia", "+591 4 4667712", "cotizaciones@tecnopartes.bo"),
        ("Distribuidora La Cancha", "+591 4 4285530", "lacancha.repuestos@gmail.com"),
        ("Global Notebook Import", "+591 3 3129944", "import@globalnotebook.com"),
        ("Comercial Santa Cruz", "+591 3 3376611", "contacto@comercialsantacruz.bo"),
        ("Zona Franca Iquique Parts", "+56 57 2412200", "sales@zfiparts.cl")
    ];

    /// <summary>
    /// One entry per product. <c>Warranty</c> indexes <see cref="WarrantyTerms"/>; <c>Cost</c> is the
    /// purchase cost in BOB that the price suggestion is built on.
    /// </summary>
    public static readonly (string Category, string Trademark, string Name, string PartNumber, decimal Cost, int Warranty)[] Products =
    [
        // Pantallas LED
        ("Pantallas LED", "HP", "Pantalla LED 15.6\" 30 pines HP 250 G7 / 255 G8", "N156BGA-EA3", 355m, 2),
        ("Pantallas LED", "HP", "Pantalla LED 14\" 30 pines HP 240 G7 / 245 G8", "N140BGA-EA4", 340m, 2),
        ("Pantallas LED", "Dell", "Pantalla LED 15.6\" 30 pines Dell Inspiron 3505", "NT156WHM-N44", 365m, 2),
        ("Pantallas LED", "Dell", "Pantalla LED 14\" 30 pines Dell Latitude 3410", "B140XTN07.2", 350m, 2),
        ("Pantallas LED", "Lenovo", "Pantalla LED 15.6\" 30 pines Lenovo IdeaPad 3 15IIL", "NT156WHM-N34", 360m, 2),
        ("Pantallas LED", "Lenovo", "Pantalla LED 14\" 30 pines Lenovo ThinkPad E14 FHD", "N140HCA-EAC", 470m, 2),
        ("Pantallas LED", "Acer", "Pantalla LED 15.6\" 30 pines Acer Aspire A315", "B156XTN08.1", 345m, 2),
        ("Pantallas LED", "Asus", "Pantalla LED 15.6\" FHD 30 pines Asus X515EA", "NV156FHM-N48", 480m, 2),
        ("Pantallas LED", "Asus", "Pantalla LED 15.6\" 144Hz Asus TUF FX505", "B156HAN08.0", 690m, 2),
        ("Pantallas LED", "Toshiba", "Pantalla LED 15.6\" 40 pines Toshiba Satellite C55", "LP156WH4-TLA1", 330m, 1),
        ("Pantallas LED", "Genérico", "Pantalla LED 17.3\" 30 pines universal HD+", "N173HCE-E31", 560m, 2),
        ("Pantallas LED", "Genérico", "Pantalla LED 13.3\" 30 pines slim universal", "N133BGE-EAB", 385m, 2),

        // Teclados
        ("Teclados", "HP", "Teclado HP 250 G7 / 255 G8 español latino negro", "L20192-161", 95m, 2),
        ("Teclados", "HP", "Teclado HP Pavilion 14 español retroiluminado", "9Z.NEESQ.201", 165m, 2),
        ("Teclados", "Dell", "Teclado Dell Inspiron 15 3567 español latino", "0KPP2C", 110m, 2),
        ("Teclados", "Dell", "Teclado Dell Latitude E7440 retroiluminado ES", "0N7HHT", 185m, 2),
        ("Teclados", "Lenovo", "Teclado Lenovo IdeaPad 320 / 330 español latino", "SN20M63000", 105m, 2),
        ("Teclados", "Lenovo", "Teclado Lenovo ThinkPad T480 con trackpoint ES", "01EN683", 220m, 2),
        ("Teclados", "Acer", "Teclado Acer Aspire A315 / E5-575 español", "NK.I1517.05T", 98m, 2),
        ("Teclados", "Asus", "Teclado Asus VivoBook 15 X512 español latino", "0KNB0-5104SP00", 120m, 2),
        ("Teclados", "Genérico", "Teclado universal notebook USB español", "KB-USB-ES", 55m, 1),

        // Baterías
        ("Baterías", "HP", "Batería HP LA04 4 celdas 14.8V 2600mAh", "728460-001", 210m, 2),
        ("Baterías", "HP", "Batería HP MU06 6 celdas 10.8V 4400mAh", "593553-001", 235m, 2),
        ("Baterías", "HP", "Batería HP HS04 / HS03 240 G4 245 G5", "807957-001", 225m, 2),
        ("Baterías", "Dell", "Batería Dell Inspiron 3421 XCMRD 4 celdas", "XCMRD", 245m, 2),
        ("Baterías", "Dell", "Batería Dell Latitude E5450 G5M10 51Wh", "G5M10", 320m, 2),
        ("Baterías", "Lenovo", "Batería Lenovo IdeaPad 320 L16C2PB2 30Wh", "L16C2PB2", 230m, 2),
        ("Baterías", "Lenovo", "Batería Lenovo ThinkPad T480 01AV489 24Wh", "01AV489", 355m, 2),
        ("Baterías", "Acer", "Batería Acer Aspire AL15A32 4 celdas", "AL15A32", 215m, 2),
        ("Baterías", "Asus", "Batería Asus X441 A31N1601 3 celdas", "A31N1601", 205m, 2),
        ("Baterías", "Genérico", "Batería universal 6 celdas 10.8V compatible", "BAT-UNI-6C", 155m, 1),

        // Cargadores
        ("Cargadores", "HP", "Cargador HP 19.5V 3.33A 65W punta azul 4.5x3.0", "710412-001", 105m, 2),
        ("Cargadores", "HP", "Cargador HP 19.5V 4.62A 90W punta azul", "710413-001", 135m, 2),
        ("Cargadores", "Dell", "Cargador Dell 19.5V 3.34A 65W punta 4.5x3.0", "LA65NM130", 115m, 2),
        ("Cargadores", "Dell", "Cargador Dell 19.5V 4.62A 90W punta 7.4x5.0", "PA-3E", 145m, 2),
        ("Cargadores", "Lenovo", "Cargador Lenovo 20V 3.25A 65W punta rectangular", "ADLX65NLC3A", 125m, 2),
        ("Cargadores", "Lenovo", "Cargador Lenovo USB-C 65W ThinkPad", "ADLX65YCC3D", 195m, 2),
        ("Cargadores", "Acer", "Cargador Acer 19V 3.42A 65W punta 5.5x1.7", "PA-1650-86", 100m, 2),
        ("Cargadores", "Asus", "Cargador Asus 19V 3.42A 65W punta 4.0x1.35", "ADP-65DW", 108m, 2),
        ("Cargadores", "Genérico", "Cargador universal 96W con 8 puntas intercambiables", "UNI-96W", 130m, 1),

        // Memorias RAM
        ("Memorias RAM", "Kingston", "Memoria RAM SODIMM DDR4 8GB 2666MHz Kingston", "KVR26S19S8/8", 215m, 3),
        ("Memorias RAM", "Kingston", "Memoria RAM SODIMM DDR4 16GB 3200MHz Kingston Fury", "KF432S20IB/16", 395m, 3),
        ("Memorias RAM", "Kingston", "Memoria RAM SODIMM DDR3L 8GB 1600MHz Kingston", "KVR16LS11/8", 185m, 3),
        ("Memorias RAM", "ADATA", "Memoria RAM SODIMM DDR4 8GB 3200MHz ADATA", "AD4S32008G22-SGN", 205m, 3),
        ("Memorias RAM", "ADATA", "Memoria RAM SODIMM DDR4 4GB 2666MHz ADATA", "AD4S26664G19-SGN", 130m, 3),
        ("Memorias RAM", "Crucial", "Memoria RAM SODIMM DDR4 16GB 2666MHz Crucial", "CT16G4SFRA266", 380m, 3),
        ("Memorias RAM", "Crucial", "Memoria RAM SODIMM DDR5 8GB 4800MHz Crucial", "CT8G48C40S5", 340m, 3),
        ("Memorias RAM", "Samsung", "Memoria RAM SODIMM DDR4 8GB 3200MHz Samsung", "M471A1K43DB1", 200m, 3),

        // Discos SSD
        ("Discos SSD", "Kingston", "SSD Kingston A400 480GB SATA 2.5\"", "SA400S37/480G", 290m, 3),
        ("Discos SSD", "Kingston", "SSD Kingston A400 240GB SATA 2.5\"", "SA400S37/240G", 195m, 3),
        ("Discos SSD", "Kingston", "SSD Kingston NV2 500GB M.2 NVMe", "SNV2S/500G", 330m, 3),
        ("Discos SSD", "Kingston", "SSD Kingston NV2 1TB M.2 NVMe", "SNV2S/1000G", 545m, 3),
        ("Discos SSD", "ADATA", "SSD ADATA SU650 480GB SATA 2.5\"", "ASU650SS-480GT", 275m, 3),
        ("Discos SSD", "ADATA", "SSD ADATA Legend 700 512GB M.2 NVMe", "ALEG-700-512GCS", 345m, 3),
        ("Discos SSD", "Crucial", "SSD Crucial BX500 500GB SATA 2.5\"", "CT500BX500SSD1", 305m, 3),
        ("Discos SSD", "Crucial", "SSD Crucial P3 1TB M.2 NVMe", "CT1000P3SSD8", 590m, 3),
        ("Discos SSD", "Western Digital", "SSD WD Green 480GB SATA 2.5\"", "WDS480G3G0A", 280m, 3),
        ("Discos SSD", "Western Digital", "SSD WD Blue SN570 1TB M.2 NVMe", "WDS100T3B0C", 620m, 3),
        ("Discos SSD", "Samsung", "SSD Samsung 870 EVO 500GB SATA 2.5\"", "MZ-77E500B", 470m, 3),

        // Discos duros HDD
        ("Discos duros HDD", "Seagate", "Disco duro Seagate BarraCuda 1TB 2.5\" SATA", "ST1000LM048", 320m, 3),
        ("Discos duros HDD", "Seagate", "Disco duro Seagate BarraCuda 500GB 2.5\" SATA", "ST500LM030", 235m, 3),
        ("Discos duros HDD", "Western Digital", "Disco duro WD Blue 1TB 2.5\" SATA 5400rpm", "WD10SPZX", 335m, 3),
        ("Discos duros HDD", "Western Digital", "Disco duro WD Blue 500GB 2.5\" SATA", "WD5000LPZX", 245m, 3),
        ("Discos duros HDD", "Toshiba", "Disco duro Toshiba MQ04 1TB 2.5\" SATA", "MQ04ABF100", 310m, 3),
        ("Discos duros HDD", "Seagate", "Disco duro externo Seagate Expansion 1TB USB 3.0", "STKM1000400", 425m, 3),

        // Bisagras
        ("Bisagras", "HP", "Bisagras HP 250 G6 / 255 G7 par izquierda-derecha", "HG-HP250G6", 42m, 1),
        ("Bisagras", "HP", "Bisagras HP Pavilion 14 par metálico", "HG-HPPAV14", 45m, 1),
        ("Bisagras", "Dell", "Bisagras Dell Inspiron 15 3567 par", "HG-DL3567", 48m, 1),
        ("Bisagras", "Lenovo", "Bisagras Lenovo IdeaPad 320 15\" par", "HG-LN320", 44m, 1),
        ("Bisagras", "Acer", "Bisagras Acer Aspire A315 par", "HG-ACA315", 40m, 1),
        ("Bisagras", "Asus", "Bisagras Asus X541 / X540 par", "HG-ASX541", 38m, 1),
        ("Bisagras", "Genérico", "Bisagras universales 15.6\" par reforzado", "HG-UNI156", 32m, 1),

        // Carcasas
        ("Carcasas", "HP", "Carcasa base inferior HP 250 G7 negra", "L49982-001", 175m, 1),
        ("Carcasas", "HP", "Carcasa tapa de pantalla HP 240 G7", "L44059-001", 190m, 1),
        ("Carcasas", "Dell", "Carcasa palmrest Dell Inspiron 3505 con touchpad", "0X0G9M", 245m, 1),
        ("Carcasas", "Lenovo", "Carcasa base inferior Lenovo IdeaPad 3 15", "5CB0X56388", 185m, 1),
        ("Carcasas", "Acer", "Carcasa palmrest Acer Aspire A315 con teclado", "6B.HE8N2.001", 235m, 1),
        ("Carcasas", "Asus", "Carcasa marco de pantalla Asus X515 bezel", "13NB0TY1AP03", 95m, 1),
        ("Carcasas", "Genérico", "Marco bezel universal 15.6\" negro", "BZ-UNI156", 88m, 1),

        // Ventiladores y coolers
        ("Ventiladores y coolers", "HP", "Ventilador HP 250 G6 / 15-BS cooler interno", "925012-001", 78m, 2),
        ("Ventiladores y coolers", "Dell", "Ventilador Dell Inspiron 15 3567 cooler", "0FX0M0", 85m, 2),
        ("Ventiladores y coolers", "Lenovo", "Ventilador Lenovo IdeaPad 320 15 cooler", "5F10S13882", 80m, 2),
        ("Ventiladores y coolers", "Acer", "Ventilador Acer Aspire A315 cooler interno", "23.GNPN7.001", 75m, 2),
        ("Ventiladores y coolers", "Asus", "Ventilador Asus TUF FX505 par CPU + GPU", "13NR00S0P01011", 165m, 2),
        ("Ventiladores y coolers", "Genérico", "Base enfriadora notebook 5 ventiladores RGB", "CP-5F-RGB", 115m, 1),
        ("Ventiladores y coolers", "Genérico", "Pasta térmica HY510 2g jeringa", "HY510-2G", 18m, 0),

        // Placas madre
        ("Placas madre", "HP", "Placa madre HP 250 G7 Intel Core i3-7020U", "L49981-601", 1180m, 2),
        ("Placas madre", "HP", "Placa madre HP 240 G7 Celeron N4000", "L44888-601", 890m, 2),
        ("Placas madre", "Dell", "Placa madre Dell Inspiron 3505 AMD Ryzen 3", "0GVMH1", 1290m, 2),
        ("Placas madre", "Lenovo", "Placa madre Lenovo IdeaPad 330 i5-8250U", "5B20R19911", 1420m, 2),
        ("Placas madre", "Acer", "Placa madre Acer Aspire A315 i3-8130U", "NB.GY411.003", 1150m, 2),
        ("Placas madre", "Asus", "Placa madre Asus X515EA i5-1135G7", "60NB0TY0-MB1", 1480m, 2),

        // Flex de video
        ("Flex de video", "HP", "Flex de video HP 250 G7 30 pines", "DD0X8LLC001", 52m, 1),
        ("Flex de video", "Dell", "Flex de video Dell Inspiron 3567 40 pines", "DC02002SR00", 58m, 1),
        ("Flex de video", "Lenovo", "Flex de video Lenovo IdeaPad 320 15 EDP", "DC02001YF10", 55m, 1),
        ("Flex de video", "Acer", "Flex de video Acer Aspire A315 30 pines", "DD0ZAULC011", 50m, 1),
        ("Flex de video", "Asus", "Flex de video Asus X541 40 pines", "1422-02F30AS", 54m, 1),
        ("Flex de video", "Genérico", "Flex de video universal 30 pines 15.6\"", "FLX-UNI30", 38m, 1),

        // Touchpads
        ("Touchpads", "HP", "Touchpad HP 250 G7 con flex", "L49983-001", 92m, 1),
        ("Touchpads", "Dell", "Touchpad Dell Inspiron 3505 con botones", "0XXXG4", 105m, 1),
        ("Touchpads", "Lenovo", "Touchpad Lenovo IdeaPad 3 15IIL", "SA469D-22HL", 98m, 1),
        ("Touchpads", "Acer", "Touchpad Acer Aspire A315 con flex", "56.HE8N2.001", 88m, 1),
        ("Touchpads", "Asus", "Touchpad Asus VivoBook X512", "90NB0KA1-R90010", 95m, 1),

        // Parlantes
        ("Parlantes", "HP", "Parlantes internos HP 250 G7 par", "L20492-001", 58m, 1),
        ("Parlantes", "Dell", "Parlantes internos Dell Inspiron 3567 par", "023CVR", 62m, 1),
        ("Parlantes", "Lenovo", "Parlantes internos Lenovo IdeaPad 320 par", "5SB0N82359", 60m, 1),
        ("Parlantes", "Acer", "Parlantes internos Acer Aspire A315 par", "23.GNPN7.002", 55m, 1),
        ("Parlantes", "Genérico", "Parlantes USB 2.0 para notebook 6W", "SPK-USB-6W", 48m, 1),

        // Cámaras web
        ("Cámaras web", "HP", "Cámara web interna HP 250 G7 HD con flex", "L20456-001", 62m, 1),
        ("Cámaras web", "Lenovo", "Cámara web interna Lenovo IdeaPad 320 HD", "5C20N00307", 65m, 1),
        ("Cámaras web", "Genérico", "Cámara web USB 1080p con micrófono", "WC-1080-USB", 145m, 2),
        ("Cámaras web", "Genérico", "Cámara web USB 720p clip universal", "WC-720-USB", 88m, 1),

        // Lectoras DVD
        ("Lectoras DVD", "HP", "Lectora DVD-RW interna HP 250 G6 SATA 9.5mm", "820286-001", 135m, 2),
        ("Lectoras DVD", "Lenovo", "Lectora DVD-RW interna Lenovo 12.7mm SATA", "GUE1N", 128m, 2),
        ("Lectoras DVD", "LG", "Lectora DVD-RW externa LG USB 2.0 slim", "GP65NB60", 175m, 2),
        ("Lectoras DVD", "Genérico", "Caddy adaptador HDD/SSD 9.5mm para bahía DVD", "CADDY-95", 45m, 1),
        ("Lectoras DVD", "Genérico", "Caddy adaptador HDD/SSD 12.7mm para bahía DVD", "CADDY-127", 45m, 1),

        // Cables y adaptadores
        ("Cables y adaptadores", "Genérico", "Cable HDMI 1.5m 4K 60Hz", "CBL-HDMI-15", 35m, 1),
        ("Cables y adaptadores", "Genérico", "Cable HDMI 3m 4K 60Hz", "CBL-HDMI-30", 55m, 1),
        ("Cables y adaptadores", "Genérico", "Adaptador USB-C a HDMI 4K", "ADP-CHDMI", 78m, 1),
        ("Cables y adaptadores", "Genérico", "Adaptador USB 3.0 a SATA 2.5\" para disco", "ADP-USB3SATA", 62m, 1),
        ("Cables y adaptadores", "Genérico", "Cable de poder notebook 220V 1.5m", "CBL-PWR-15", 28m, 1),
        ("Cables y adaptadores", "Genérico", "Hub USB 3.0 de 4 puertos", "HUB-USB3-4P", 68m, 1),
        ("Cables y adaptadores", "Genérico", "Adaptador miniDisplayPort a VGA", "ADP-MDPVGA", 72m, 1),
        ("Cables y adaptadores", "Genérico", "Cable de red UTP Cat6 3m armado", "CBL-UTP6-30", 25m, 1),

        // Accesorios
        ("Accesorios", "Genérico", "Mouse óptico USB 1000dpi negro", "MS-USB-1000", 32m, 1),
        ("Accesorios", "Genérico", "Mouse inalámbrico 2.4GHz 1600dpi", "MS-WL-1600", 58m, 2),
        ("Accesorios", "Genérico", "Mochila para notebook 15.6\" acolchada", "BAG-156", 125m, 1),
        ("Accesorios", "Genérico", "Maletín para notebook 14\" con correa", "BAG-14", 98m, 1),
        ("Accesorios", "Genérico", "Kit de destornilladores de precisión 32 en 1", "TOOL-32IN1", 85m, 2),
        ("Accesorios", "Genérico", "Protector de teclado silicona 15.6\"", "KBC-156", 22m, 0),
        ("Accesorios", "Genérico", "Limpiador de pantallas kit con paño microfibra", "CLN-KIT", 30m, 0),
        ("Accesorios", "Genérico", "Soporte ergonómico de aluminio para notebook", "STD-ALU", 165m, 2),
        ("Accesorios", "Kingston", "Pendrive Kingston DataTraveler 64GB USB 3.2", "DTX/64GB", 68m, 3),
        ("Accesorios", "Kingston", "Pendrive Kingston DataTraveler 128GB USB 3.2", "DTX/128GB", 105m, 3),
        ("Accesorios", "ADATA", "Memoria microSD ADATA 64GB clase 10", "AUSDX64GUICL10", 72m, 3),
        ("Accesorios", "Samsung", "Memoria microSD Samsung EVO Select 128GB", "MB-ME128KA", 128m, 3),
        ("Accesorios", "Genérico", "Audífonos con micrófono para notebook 3.5mm", "HP-35MM", 45m, 1),
        ("Accesorios", "Genérico", "Webcam privacy cover pack x3", "WCC-X3", 15m, 0),
        ("Accesorios", "Genérico", "Alfombrilla mouse pad grande 80x30cm", "PAD-8030", 55m, 1)
    ];

    /// <summary>Individuals: first name, last name, mother's last name, city.</summary>
    public static readonly (string FirstName, string LastName, string MotherLastName, string City)[] IndividualClients =
    [
        ("Juan Carlos", "Mamani", "Choque", "Cochabamba"),
        ("María Elena", "Flores", "Condori", "Cochabamba"),
        ("Luis Fernando", "Rojas", "Vargas", "Cochabamba"),
        ("Ana Gabriela", "Terceros", "Salazar", "Cochabamba"),
        ("Rodrigo", "Peñaranda", "Guzmán", "Cochabamba"),
        ("Silvia", "Colque", "Apaza", "La Paz"),
        ("Marco Antonio", "Zeballos", "Ferrufino", "Cochabamba"),
        ("Patricia", "Arispe", "Montaño", "Santa Cruz"),
        ("Edwin", "Huanca", "Quispe", "El Alto"),
        ("Gabriela", "Ledezma", "Careaga", "Cochabamba"),
        ("Nelson", "Cuellar", "Justiniano", "Santa Cruz"),
        ("Verónica", "Aguilar", "Sejas", "Cochabamba"),
        ("Freddy", "Ticona", "Nina", "La Paz"),
        ("Lucía", "Antezana", "Camacho", "Cochabamba"),
        ("Javier", "Ovando", "Suárez", "Santa Cruz"),
        ("Roxana", "Villarroel", "Claros", "Cochabamba"),
        ("Álvaro", "Balderrama", "Orellana", "Cochabamba"),
        ("Ximena", "Encinas", "Paz", "Sucre")
    ];

    /// <summary>Companies: legal name, contact person, city.</summary>
    public static readonly (string Name, string ContactName, string City)[] CompanyClients =
    [
        ("Servicios Informáticos del Valle S.R.L.", "Ing. Marcelo Céspedes", "Cochabamba"),
        ("Colegio San Agustín", "Lic. Rosario Medina", "Cochabamba"),
        ("Constructora Illimani S.A.", "Arq. Pablo Iriarte", "La Paz"),
        ("Farmacorp Sucursal Cochabamba", "Sra. Elena Barrientos", "Cochabamba"),
        ("Cyber Estrella Ltda.", "Sr. Wilson Achá", "Cochabamba"),
        ("Transportes Chapare S.R.L.", "Sr. Hugo Delgado", "Cochabamba"),
        ("Instituto Técnico Ayacucho", "Lic. Carla Fernández", "Cochabamba")
    ];

    public static readonly string[] Streets =
    [
        "Av. Ayacucho", "Calle España", "Av. Heroínas", "Calle Junín", "Av. Blanco Galindo km 4",
        "Av. América", "Calle Lanza", "Av. Oquendo", "Calle Baptista", "Av. Villazón km 2"
    ];

    public static readonly string[] AdjustmentNotes =
    [
        "Ajuste por conteo físico mensual",
        "Merma por unidad dañada en almacén",
        "Corrección de error de digitación en compra",
        "Devolución de cliente reingresada a stock",
        "Unidad de muestra retirada para exhibición",
        "Faltante detectado en inventario semanal"
    ];

    public static readonly string[] TransferNotes =
    [
        "Reposición de stock solicitada por la sucursal",
        "Traslado por alta rotación en mostrador",
        "Redistribución tras conteo de inventario",
        "Envío para cubrir pedido de cliente corporativo"
    ];

    /// <summary>Requests for items that may not be in the catalog ("encargos").</summary>
    public static readonly (string Description, decimal Price)[] OrderRequests =
    [
        ("Pantalla LED 15.6\" táctil para HP Envy x360, 30 pines", 980m),
        ("Teclado retroiluminado para MacBook Pro 13\" A1502 español", 620m),
        ("Batería original Dell XPS 13 9370 modelo G8VCF", 890m),
        ("Placa madre Lenovo Legion 5 con RTX 3050", 3450m),
        ("Bisagras completas para Asus ZenBook UX430 par", 210m),
        ("Cargador original Microsoft Surface Pro 65W", 540m),
        ("Disco SSD NVMe 2TB Samsung 980 Pro", 1450m),
        ("Pantalla OLED 14\" para Asus ZenBook UX3402", 1890m),
        ("Carcasa completa MacBook Air 13\" A2337", 1250m),
        ("Memoria RAM SODIMM DDR5 32GB 5600MHz", 980m),
        ("Ventilador original Acer Nitro 5 AN515-55 par", 320m),
        ("Flex de video para Dell Latitude 7490 táctil", 275m)
    ];

    /// <summary>Quotation lines ("cotizaciones"), usually multi-item.</summary>
    public static readonly (string Detail, decimal Price)[] Quotes =
    [
        ("Repotenciación de 10 equipos: SSD 480GB + RAM 8GB DDR4 por unidad, incluye instalación y clonado", 8900m),
        ("Cambio de pantalla LED 15.6\" HP 250 G7 con mano de obra e IVA incluido", 620m),
        ("Mantenimiento preventivo de 15 notebooks: limpieza interna, cambio de pasta térmica y revisión", 2250m),
        ("Provisión de 20 mouse inalámbricos y 20 alfombrillas para oficina", 2280m),
        ("Kit de repuestos para laboratorio: 5 teclados ES, 5 baterías HP MU06, 5 cargadores 65W", 3350m),
        ("Actualización a SSD NVMe 1TB + RAM 16GB en Lenovo ThinkPad E14", 1420m),
        ("Reparación de placa madre Acer Aspire A315 con reballing de chip gráfico", 890m),
        ("Suministro de 12 pendrives 64GB y 6 discos externos 1TB", 3390m),
        ("Cambio de bisagras y carcasa base HP Pavilion 14 con mano de obra", 480m),
        ("Instalación de 8 cámaras web 1080p y audífonos para sala de videoconferencia", 1840m),
        ("Recuperación de datos de disco 1TB con sectores dañados", 750m),
        ("Provisión de 30 cables HDMI 1.5m y 10 adaptadores USB-C a HDMI", 2340m),
        ("Armado de 4 estaciones de trabajo con monitores y accesorios", 12600m),
        ("Cambio de teclado y touchpad Dell Inspiron 3505 con garantía de 6 meses", 395m),
        ("Diagnóstico y limpieza de 25 equipos del laboratorio de computación", 3125m)
    ];
}
