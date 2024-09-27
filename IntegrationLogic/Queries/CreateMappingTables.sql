BEGIN TRY
	
	-- Bond Mapping Tables
	CREATE TABLE [dbo].[MajescoBondMapping](
		[UniqBondMapping] [int] IDENTITY(10000,1) NOT NULL,
		[ReceivableCode] [varchar](max) NULL,
		[FormNo] [varchar](max) NULL,
		[ShortNotes] [varchar](max) NULL,
		[LineType] [varchar](max) NULL,
		[TransactionCode] [varchar](max) NULL,
		CONSTRAINT [PK_MajescoBondMapping] PRIMARY KEY CLUSTERED 
	(
		[UniqBondMapping] ASC
	)WITH (IGNORE_DUP_KEY = OFF) ON [PRIMARY]
	) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY];

	-- Bond Mapping Indexes
	CREATE NONCLUSTERED INDEX Idx_BondMapping_All
		ON dbo.MajescoBondMapping (UniqBondMapping) INCLUDE (ReceivableCode, FormNo, LineType);

	-- Company Mapping Tables
	CREATE TABLE [dbo].[MajescoCompanyMapping](
		[UniqCompanyMapping] [int] IDENTITY(10000,1) NOT NULL,
		[UnderwritingCompanyCode] [varchar](max) NULL,
		[UniqCompany] [int] NULL,
		[LookupCode] [varchar](max) NULL,
		[NameOf] [varchar](max) NULL,
		CONSTRAINT [PK_MajescoCompanyMapping] PRIMARY KEY CLUSTERED 
	(
		[UniqCompanyMapping] ASC
	)WITH (IGNORE_DUP_KEY = OFF) ON [PRIMARY]
	) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY];

	-- Company Mapping Indexes
	CREATE NONCLUSTERED INDEX Idx_CompanyMapping_All
		ON dbo.MajescoCompanyMapping (UniqCompanyMapping) INCLUDE (UnderwritingCompanyCode, UniqCompany, LookupCode);

	CREATE NONCLUSTERED INDEX Idx_EpicCompany_LookupCode
		ON dbo.Company (UniqEntity) INCLUDE (LookupCode, NameOf);

	SET NOCOUNT ON;

	-- Bond Mapping Initial Inserts
	INSERT INTO MajescoBondMapping VALUES ('NUSC', 'BMC84', 'FMCSA PROPERTY BROKERS BOND', 'BMC', '');
	INSERT INTO MajescoBondMapping VALUES ('NUSC', 'BMCFF', 'BMC84 Rev10.1.13 Freight Fwdr', 'BMC', '');
	INSERT INTO MajescoBondMapping VALUES ('NUSC', 'BMCFR', 'New Language Rider 10.1.13 FF', 'BMC', '');
	INSERT INTO MajescoBondMapping VALUES ('NUSC', 'BMCPB', 'BMC84 Rev10.1.13 Prop Brk', 'BMC', '');
	INSERT INTO MajescoBondMapping VALUES ('NUSC', 'BMCPR', 'New Language Rider 10.1.13', 'BMC', '');
	INSERT INTO MajescoBondMapping VALUES ('NUSC', 'BMCXS', 'EXCESS PROPERTY BROKER BOND EXCESS', 'BMC', '');
	INSERT INTO MajescoBondMapping VALUES ('NUSC', 'TGO85', 'FMCSA TRUST FUND', 'BMC', '');
	INSERT INTO MajescoBondMapping VALUES ('CBFEE', 'BMCPB', 'bond fee', 'BMC', 'AFEE');
	INSERT INTO MajescoBondMapping VALUES ('SRFEE', 'BMCPB', 'SPECIAL RISK FEE', 'BMC', 'AFEE');
	INSERT INTO MajescoBondMapping VALUES ('CBFEE', 'BMCFF', 'BOND FEE', 'BMC', 'AFEE');
	INSERT INTO MajescoBondMapping VALUES ('IPFEE', 'BMCPB', 'BOND FEE', 'BMC', 'AFEE');
	INSERT INTO MajescoBondMapping VALUES ('BSFEE', 'PMCPB', 'BOND FEE', 'BMC', 'AFEE');
	INSERT INTO MajescoBondMapping VALUES ('BSFEE', 'BMCPB', 'BOND FEE', 'BMC', 'AFEE');
	INSERT INTO MajescoBondMapping VALUES ('STAX', 'BMCPB', 'KY Tax', 'BMC', 'GOVT');
	INSERT INTO MajescoBondMapping VALUES ('CTAX', 'BMCPB', 'KY MUNICIPAL', 'BMC', '');
	INSERT INTO MajescoBondMapping VALUES ('CTB', 'C0028', 'CUSTODIAN OF BONDED MERCHANDISE', 'CBM', '');
	INSERT INTO MajescoBondMapping VALUES ('CBFEE', 'C0028', 'Continuous Bond Fee', 'CBM', 'AFEE');
	INSERT INTO MajescoBondMapping VALUES ('BSFEE', 'C0028', 'Bond Service Fee', 'CBM', 'AFEE');
	INSERT INTO MajescoBondMapping VALUES ('SRFEE', 'C0028', 'Special Risk Program', 'CBM', 'AFEE');
	INSERT INTO MajescoBondMapping VALUES ('IPFEE', 'C0028', 'Initial Transaction Fee', 'CBM', 'AFEE');
	INSERT INTO MajescoBondMapping VALUES ('NUSC', 'CMAB8', 'CARNET COUNCIL MEMBERS ANNUAL BOND', 'CCTB', '');
	INSERT INTO MajescoBondMapping VALUES ('CBFEE', 'CMAB8', 'CARNET COUNCIL MEMBERS ANNUAL BOND FEE', 'CCTB', 'AFEE');
	INSERT INTO MajescoBondMapping VALUES ('CTB', 'C0048', 'FOREIGN TRADE ZONE OPERATOR', 'CFTZ', '');
	INSERT INTO MajescoBondMapping VALUES ('BSFEE', 'C0048', 'BOND SERVICE FEE', 'CFTZ', 'AFEE');
	INSERT INTO MajescoBondMapping VALUES ('CBFEE', 'C0048', 'BOND FEE', 'CFTZ', 'AFEE');
	INSERT INTO MajescoBondMapping VALUES ('CTB', 'CA1CB', 'CANADIAN', 'CNCB', '');
	INSERT INTO MajescoBondMapping VALUES ('CTB', 'CA1I3', 'Canadian', 'CNCB', '');
	INSERT INTO MajescoBondMapping VALUES ('CTB', 'CA1I5', 'CANADIAN', 'CNCB', '');
	INSERT INTO MajescoBondMapping VALUES ('CTB', 'CA1IM', 'Canadian Import Bond', 'CNCB', '');
	INSERT INTO MajescoBondMapping VALUES ('CTB', 'CA1TI', 'Canadian TIB', 'CNCB', '');
	INSERT INTO MajescoBondMapping VALUES ('CTB', 'CA2AC', 'Canadian Bonded Air Carrier', 'CNCB', '');
	INSERT INTO MajescoBondMapping VALUES ('CTB', 'CA2BW', 'Canadian Bonded Warehouse', 'CNCB', '');
	INSERT INTO MajescoBondMapping VALUES ('CTB', 'CA2FF', 'Canadian Freight Forwarder', 'CNCB', '');
	INSERT INTO MajescoBondMapping VALUES ('CTB', 'CA2HC', 'Canadian Hwy Carrier', 'CNCB', '');
	INSERT INTO MajescoBondMapping VALUES ('CTB', 'CA2MC', 'Canadian Marine Carrier', 'CNCB', '');
	INSERT INTO MajescoBondMapping VALUES ('CTB', 'CA2RC', 'Canadian Rail Carrier', 'CNCB', '');
	INSERT INTO MajescoBondMapping VALUES ('CTB', 'CA2SW', 'CANADIAN BOND', 'CNCB', '');
	INSERT INTO MajescoBondMapping VALUES ('CTB', 'CACBM', 'Canadian Carnet Multiterm', 'CNCB', '');
	INSERT INTO MajescoBondMapping VALUES ('CTB', 'CAGST', 'Canadian Customs Bond', 'CNCB', '');
	INSERT INTO MajescoBondMapping VALUES ('BCS', 'CA2FF', 'BOND FEE', 'CNCB', 'AFEE');
	INSERT INTO MajescoBondMapping VALUES ('BSFEE', 'CA2FF', 'BOND FEE', 'CNCB', 'AFEE');
	INSERT INTO MajescoBondMapping VALUES ('CBFEE', 'CA1CB', 'CANADIAN', 'CNCB', 'AFEE');
	INSERT INTO MajescoBondMapping VALUES ('CBFEE', 'CA1I1', 'CANADIAN', 'CNCB', 'AFEE');
	INSERT INTO MajescoBondMapping VALUES ('CBFEE', 'CA1I3', 'Canadian', 'CNCB', 'AFEE');
	INSERT INTO MajescoBondMapping VALUES ('CBFEE', 'CA1IM', 'BOND FEE', 'CNCB', 'AFEE');
	INSERT INTO MajescoBondMapping VALUES ('CBFEE', 'CA1TI', 'BOND FEE', 'CNCB', 'AFEE');
	INSERT INTO MajescoBondMapping VALUES ('CBFEE', 'CA2AC', 'BOND FEE', 'CNCB', 'AFEE');
	INSERT INTO MajescoBondMapping VALUES ('CBFEE', 'CA2BW', 'BOND FEE', 'CNCB', 'AFEE');
	INSERT INTO MajescoBondMapping VALUES ('CBFEE', 'CA2FF', 'BOND FEE', 'CNCB', 'AFEE');
	INSERT INTO MajescoBondMapping VALUES ('CBFEE', 'CA2HC', 'BOND FEE', 'CNCB', 'AFEE');
	INSERT INTO MajescoBondMapping VALUES ('CBFEE', 'CA2MC', 'BOND FEE', 'CNCB', 'AFEE');
	INSERT INTO MajescoBondMapping VALUES ('CBFEE', 'CA2RC', 'BOND FEE', 'CNCB', 'AFEE');
	INSERT INTO MajescoBondMapping VALUES ('CBFEE', 'CACBM', 'BOND FEE', 'CNCB', 'AFEE');
	INSERT INTO MajescoBondMapping VALUES ('CBFEE', 'CAGST', 'Bond Fee', 'CNCB', 'AFEE');
	INSERT INTO MajescoBondMapping VALUES ('CTB', 'C0010', 'IMP/BRKR CTB - SCH3/4S SECTION  XI', 'CTB', '');
	INSERT INTO MajescoBondMapping VALUES ('CTB', 'C0011', 'IMP/BRKR TEMPORARY IMPORTATION CTB', 'CTB', '');
	INSERT INTO MajescoBondMapping VALUES ('CTB', 'C0012', 'IMP/BRKR WAREHOUSE CTB', 'CTB', '');
	INSERT INTO MajescoBondMapping VALUES ('CTB', 'C0013', 'IMP/BRKR AUTO CTB', 'CTB', '');
	INSERT INTO MajescoBondMapping VALUES ('CTB', 'C0014', 'IMP/BRKR ADD/CVD CTB', 'CTB', '');
	INSERT INTO MajescoBondMapping VALUES ('CTB', 'C0015', 'IMP/BRKR FDA CTB', 'CTB', '');
	INSERT INTO MajescoBondMapping VALUES ('CTB', 'C0015', 'BOND', 'CTB', '');
	INSERT INTO MajescoBondMapping VALUES ('CTB', 'C0016', 'IMP/BRKR SCH8 PT1 1/4S CHPTR98 CTB', 'CTB', '');
	INSERT INTO MajescoBondMapping VALUES ('CTB', 'C0017', 'IMP/BRKR CBI/GSP CTB', 'CTB', '');
	INSERT INTO MajescoBondMapping VALUES ('CTB', 'C0018', 'IMP/BRKR GENERAL MERCH. CTB', 'CTB', '');
	INSERT INTO MajescoBondMapping VALUES ('CTB', 'C001G', 'CTB AGRICULTUE ADD OLD FORM CAG14', 'CTB', '');
	INSERT INTO MajescoBondMapping VALUES ('CTB', 'C001Q', 'CTB - AQUACULTURE   OLD FORM CAQ14', 'CTB', '');
	INSERT INTO MajescoBondMapping VALUES ('CTB', 'C001R', 'IMP/BRKR RECONCILIATION RIDER', 'CTB', '');
	INSERT INTO MajescoBondMapping VALUES ('CTB', 'C001T', 'CTB AGRICULTURE OLD FORM CAG14', 'CTB', '');
	INSERT INTO MajescoBondMapping VALUES ('CTB', 'C0058', 'PUBLIC GAUGER', 'CTB', '');
	INSERT INTO MajescoBondMapping VALUES ('CTB', 'C0118', 'AIRPORT CUSTOMS SECURITY BOND', 'CTB', '');
	INSERT INTO MajescoBondMapping VALUES ('CTB', 'C0118', 'AIRPORT CUSTOS', 'CTB', '');
	INSERT INTO MajescoBondMapping VALUES ('CTB', 'C0138', 'IMMIGRATION BOND', 'CTB', '');
	INSERT INTO MajescoBondMapping VALUES ('CTB', 'C0148', 'MIAMI IN-BOND CONSOLIDATOR', 'CTB', '');
	INSERT INTO MajescoBondMapping VALUES ('CTB', 'C0158', 'INTELLECTUAL PROPERTY BOND', 'CTB', '');
	INSERT INTO MajescoBondMapping VALUES ('CTB', 'C0168', 'IMPORTER SECURITY FILING (ISF)', 'CTB', '');
	INSERT INTO MajescoBondMapping VALUES ('CTB', 'C0178', 'MARINE TERMINAL OPERATOR', 'CTB', '');
	INSERT INTO MajescoBondMapping VALUES ('CTB', 'C0208', 'EXPORT CONSOLIDATOR BOND', 'CTB', '');
	INSERT INTO MajescoBondMapping VALUES ('CTB', 'C33A8', 'INTERNATIONAL CARRIER AND IIT', 'CTB', '');
	INSERT INTO MajescoBondMapping VALUES ('CTB', 'CAG14', 'IMP/BRKR CTB -AGRICULTURE PRODUCTS', 'CTB', '');
	INSERT INTO MajescoBondMapping VALUES ('CTB', 'CAQ14', 'IMP/BRKR CTB - AQUACULTURE PRODUCTS', 'CTB', '');
	INSERT INTO MajescoBondMapping VALUES ('CTB', 'CN028', '', 'CTB', '');
	INSERT INTO MajescoBondMapping VALUES ('BSFEE', 'C0010', 'BOND SERVICE FEE', 'CTB', 'AFEE');
	INSERT INTO MajescoBondMapping VALUES ('BSFEE', 'C0015', 'Bond Service Fee', 'CTB', 'AFEE');
	INSERT INTO MajescoBondMapping VALUES ('BSFEE', 'C0018', 'Bond Service Fee', 'CTB', 'AFEE');
	INSERT INTO MajescoBondMapping VALUES ('IPFEE', 'C0018', 'Bond Service Fee', 'CTB', 'AFEE');
	INSERT INTO MajescoBondMapping VALUES ('SRFEE', 'C0018', 'Bond Service Fee', 'CTB', 'AFEE');
	INSERT INTO MajescoBondMapping VALUES ('BSFEE', 'CN028', 'Canadian Bond Fee', 'CTB', 'AFEE');
	INSERT INTO MajescoBondMapping VALUES ('CBFEE', 'C0010', 'Continuous Bond Fee', 'CTB', 'AFEE');
	INSERT INTO MajescoBondMapping VALUES ('CBFEE', 'C0014', 'Continuous Bond Fee', 'CTB', 'AFEE');
	INSERT INTO MajescoBondMapping VALUES ('CBFEE', 'C0015', 'Continuous Bond Fee', 'CTB', 'AFEE');
	INSERT INTO MajescoBondMapping VALUES ('CBFEE', 'C0017', 'bond fee', 'CTB', 'AFEE');
	INSERT INTO MajescoBondMapping VALUES ('CBFEE', 'C0018', 'Continuous Bond Fee', 'CTB', 'AFEE');
	INSERT INTO MajescoBondMapping VALUES ('CBFEE', 'C0118', 'AIRPORT BOND FEE', 'CTB', 'AFEE');
	INSERT INTO MajescoBondMapping VALUES ('CBFEE', 'C01A8', 'BOND FEE', 'CTB', 'AFEE');
	INSERT INTO MajescoBondMapping VALUES ('CBFEE', 'C03A8', 'Bond Fee', 'CTB', 'AFEE');
	INSERT INTO MajescoBondMapping VALUES ('CBFEE', 'CN028', 'Canadian bond fee', 'CTB', 'AFEE');
	INSERT INTO MajescoBondMapping VALUES ('SNUSC', 'DOT10', 'DOT', 'DOT', '');
	INSERT INTO MajescoBondMapping VALUES ('SBFEE', 'DOT10', 'DOT Fee', 'DOT', 'AFEE');
	INSERT INTO MajescoBondMapping VALUES ('CTB', 'C01A8', 'IMP/BRKR DRAWBACK PAYMENT REFUNDS', 'DRBK', '');
	INSERT INTO MajescoBondMapping VALUES ('SBFEE', 'C01A8', 'IMP/BRKR DRAWBACK PAYMENT REFUNDS FEE', 'DRBK', 'AFEE');
	INSERT INTO MajescoBondMapping VALUES ('NUSC', 'OTIFF', 'OTI former FMC59, FMC00', 'FF', '');
	INSERT INTO MajescoBondMapping VALUES ('IPFEE', 'OTIFF', 'Initial Processing Fee', 'FF', 'AFEE');
	INSERT INTO MajescoBondMapping VALUES ('SRFEE', 'OTIFF', 'Special Risk Fee', 'FF', 'AFEE');
	INSERT INTO MajescoBondMapping VALUES ('CBFEE', 'OTIFF', 'BOND FEE', 'FF', 'AFEE');
	INSERT INTO MajescoBondMapping VALUES ('BSFEE', 'OTIFF', 'BOND SERVICE FEE', 'FF', 'AFEE');
	INSERT INTO MajescoBondMapping VALUES ('NUSC', 'FTZO0', 'FOREIGN TRADE ZONE OPERATOR', 'FTZ', '');
	INSERT INTO MajescoBondMapping VALUES ('NUSC', 'FTZU0', 'FOREIGN TRADE ZONE USER', 'FTZ', '');
	INSERT INTO MajescoBondMapping VALUES ('NUSC', 'FTZUO', '', 'FTZ', '');
	INSERT INTO MajescoBondMapping VALUES ('CTB', 'C0038', 'INTERNATIONAL CARRIER', 'IC', '');
	INSERT INTO MajescoBondMapping VALUES ('CBFEE', 'C0038', 'Continuous Bond Fee', 'IC', 'AFEE');
	INSERT INTO MajescoBondMapping VALUES ('BSFEE', 'C0038', 'Bond Service Fee', 'IC', 'AFEE');
	INSERT INTO MajescoBondMapping VALUES ('SRFEE', 'C0038', 'SPECIAL RISK', 'IC', 'AFEE');
	INSERT INTO MajescoBondMapping VALUES ('IPFEE', 'C0038', 'Intial fee', 'IC', 'AFEE');
	INSERT INTO MajescoBondMapping VALUES ('CTB', 'C03A8', 'INSTRUMENTS OF INTL TRAFFIC', 'IIT', '');
	INSERT INTO MajescoBondMapping VALUES ('CBFEE', 'C03A8', 'Bond Fee', 'IIT', 'AFEE');
	INSERT INTO MajescoBondMapping VALUES ('NUSC', 'BATF8', 'BUREAU OF ALCOHOL, TOBACCO, FIREARM', 'NUSC', '');
	INSERT INTO MajescoBondMapping VALUES ('NUSC', 'F132B', 'FMC - Passenger Vessel Surety Bond', 'NUSC', '');
	INSERT INTO MajescoBondMapping VALUES ('NUSC', 'MSDDC', 'MILITARY SURFACE DEPLOY/DISTR. CMND', 'NUSC', '');
	INSERT INTO MajescoBondMapping VALUES ('NUSC', 'FGB05', 'Misc Bond', 'NUSC', '');
	INSERT INTO MajescoBondMapping VALUES ('NUSC', 'BATF8', 'BUREAU OF ALCOHOL, TOBACCO, FIREARM', 'NUSC', '');
	INSERT INTO MajescoBondMapping VALUES ('CBFEE', 'BATF8', 'Continuous Bond Fee', 'NUSC', 'AFEE');
	INSERT INTO MajescoBondMapping VALUES ('NUSC', 'OTICR', 'OTI CHINA RIDER', 'NVOC', '');
	INSERT INTO MajescoBondMapping VALUES ('NUSC', 'OTINV', 'OTI BOND former FMC48 NVOCC NVOFR', 'NVOC', '');
	INSERT INTO MajescoBondMapping VALUES ('NUSC', 'ETRVI', 'Virgin Islands tax ride', 'NVOC', 'GOVT');
	INSERT INTO MajescoBondMapping VALUES ('BSFEE', 'OTINV', 'Bond Service Fee', 'NVOC', 'AFEE');
	INSERT INTO MajescoBondMapping VALUES ('IPFEE', 'OTINV', 'Initial Processing Fee', 'NVOC', 'AFEE');
	INSERT INTO MajescoBondMapping VALUES ('SRFEE', 'OTINV', 'special risk fee', 'NVOC', 'AFEE');
	INSERT INTO MajescoBondMapping VALUES ('CBFEE', 'OTINV', 'Bond Fee', 'NVOC', 'AFEE');
	INSERT INTO MajescoBondMapping VALUES ('STB', 'S0010', 'IMP/BRKR SCH3/4S SECTION XI', 'STB', '');
	INSERT INTO MajescoBondMapping VALUES ('STB', 'S0011', 'TEMP IMPORTATION', 'STB', '');
	INSERT INTO MajescoBondMapping VALUES ('STB', 'S0012', 'WAREHOUSE', 'STB', '');
	INSERT INTO MajescoBondMapping VALUES ('STB', 'S0013', 'IMP/BRKR  AUTO', 'STB', '');
	INSERT INTO MajescoBondMapping VALUES ('STB', 'S0014', 'IMP/BRKR ADD/CVD', 'STB', '');
	INSERT INTO MajescoBondMapping VALUES ('STB', 'S0015', 'IMP/BRKR FDA', 'STB', '');
	INSERT INTO MajescoBondMapping VALUES ('STB', 'S0016', 'IMP/BRKR SCH8 PT1 1/4S CHPTR 98', 'STB', '');
	INSERT INTO MajescoBondMapping VALUES ('STB', 'S0017', 'IMP/BRKR CBI/GSP', 'STB', '');
	INSERT INTO MajescoBondMapping VALUES ('STB', 'S0018', 'IMP/BRKR GENERAL MERCHANDISE', 'STB', '');
	INSERT INTO MajescoBondMapping VALUES ('STB', 'S0038', 'SINGLE INTERNATIONAL CARRIER', 'STB', '');
	INSERT INTO MajescoBondMapping VALUES ('STB', 'S0068', 'SINGLE WOOL & FUR PRODUCTS LABELING', 'STB', '');
	INSERT INTO MajescoBondMapping VALUES ('STB', 'S0078', 'BILL OF LADING', 'STB', '');
	INSERT INTO MajescoBondMapping VALUES ('STB', 'S0088', 'DETENTION OF COPYRIGHTED MATERIAL', 'STB', '');
	INSERT INTO MajescoBondMapping VALUES ('STB', 'S0098', 'NEUTRALITY (SINGLE ENTRY ONLY)', 'STB', '');
	INSERT INTO MajescoBondMapping VALUES ('STB', 'S0108', 'COURT COSTS FOR CONDEMNED GOODS', 'STB', '');
	INSERT INTO MajescoBondMapping VALUES ('STB', 'S0128', 'ITC EXCLUSION', 'STB', '');
	INSERT INTO MajescoBondMapping VALUES ('STB', 'S0168', 'IMPORTER SECURITY FILING', 'STB', '');
	INSERT INTO MajescoBondMapping VALUES ('STB', 'S01A8', 'IMP/BRKR DRAWBACK PAYMENT REFUNDS', 'STB', '');
	INSERT INTO MajescoBondMapping VALUES ('STB', 'S03A8', 'INSTRUMENTS OF INTL TRAFFIC', 'STB', '');
	INSERT INTO MajescoBondMapping VALUES ('BSFEE', 'S0108', 'Bond Service Fee', 'STB', 'AFEE');
	INSERT INTO MajescoBondMapping VALUES ('BSFEE', 'S0128', 'Bond Service Fee', 'STB', 'AFEE');
	INSERT INTO MajescoBondMapping VALUES ('SBFEE', 'S0015', 'IMP/BRKR FDA', 'STB', 'AFEE');
	INSERT INTO MajescoBondMapping VALUES ('CTAX', 'S0018', 'Kentucky Municipal Tax', 'STB', 'GOVT');
	INSERT INTO MajescoBondMapping VALUES ('STAX', 'S0018', 'Kentucky State Tax', 'STB', 'GOVT');
	INSERT INTO MajescoBondMapping VALUES ('NUSC', 'MS001', 'MISCELLANEOUS SURETY BOND', 'SURT', '');
	INSERT INTO MajescoBondMapping VALUES ('BSFEE', 'MS001', 'Bond Service Fee', 'SURT', 'AFEE');
	INSERT INTO MajescoBondMapping VALUES ('CBFEE', 'MS001', 'Continuous Bond Fee', 'SURT', 'AFEE');

	UPDATE MajescoBondMapping
	SET TransactionCode = NULL
	WHERE TransactionCode = '';

	-- Company Mapping Initial Inserts
	INSERT INTO MajescoCompanyMapping VALUES ('281',-1,'MAT281','MATSON-CHARLTON SURETY GROUP');
	INSERT INTO MajescoCompanyMapping VALUES ('119',-1,'GUA119','Guarantee Co. of North America');
	INSERT INTO MajescoCompanyMapping VALUES ('462',-1,'LEX462','Lexington National Insurance');
	INSERT INTO MajescoCompanyMapping VALUES ('891',-1,'NAS893','Swiss Re/NAS Surety');
	INSERT INTO MajescoCompanyMapping VALUES ('INT700',-1,'INT700','Intact Canada - formerly GCNA Canada');
	INSERT INTO MajescoCompanyMapping VALUES ('MOO403',-1,'MOO403','McLean Hallmark Insurance Group');
	INSERT INTO MajescoCompanyMapping VALUES ('RIGCAN',-1,'RIGCAN','Roanoke Canada');
	INSERT INTO MajescoCompanyMapping VALUES ('TEMPLE',-1,'TEMPLE','TEMPLE INSURANCE COMPANY');

	SET NOCOUNT OFF;

END TRY

BEGIN CATCH

	SELECT 'Line: ' + CONVERT(VARCHAR, ERROR_LINE()) + ', Message: ' + ERROR_MESSAGE() AS ErrorMessage;

END CATCH;