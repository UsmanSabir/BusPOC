USE [EposDb_MVF_Server]
GO
/****** Object:  Table [dbo].[OrderBookingInfo]    Script Date: 29/05/2019 10:55:54 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[OrderBookingInfo](
	[OrderAID] [int] NOT NULL,
	[OrderLID] [smallint] NOT NULL,
	[BookedByAID] [int] NULL,
	[BookedByLID] [smallint] NULL,
	[BookedBy] [nvarchar](200) NULL,
	[ServedByAID] [int] NULL,
	[ServerdByLID] [smallint] NULL,
	[RecordAdded] [bit] NULL,
	[RecordEdited] [bit] NULL,
	[LocationsIdString] [varchar](4000) NULL,
 CONSTRAINT [PK_OrderBookingInfo] PRIMARY KEY CLUSTERED 
(
	[OrderAID] ASC,
	[OrderLID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[OrderItems]    Script Date: 29/05/2019 10:55:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[OrderItems](
	[OrderItemAID] [int] IDENTITY(1,1) NOT NULL,
	[OrderItemLID] [smallint] NOT NULL,
	[OrderAID] [int] NOT NULL,
	[OrderLID] [smallint] NOT NULL,
	[ProductAID] [int] NULL,
	[ProductLID] [smallint] NULL,
	[SeatAID] [int] NULL,
	[SeatLID] [smallint] NULL,
	[Name] [nvarchar](100) NOT NULL,
	[Quantity] [smallint] NOT NULL,
	[Price] [money] NOT NULL,
	[LineTotalAfterDiscount] [decimal](19, 11) NOT NULL,
	[VAT] [bit] NOT NULL,
	[VATCode] [nchar](2) NOT NULL,
	[VATRate] [decimal](9, 3) NULL,
	[VATAmount] [decimal](19, 11) NOT NULL,
	[ProductType] [tinyint] NOT NULL,
	[UserAID] [int] NOT NULL,
	[UserLID] [smallint] NOT NULL,
	[DeferBy] [smallint] NULL,
	[TimeStamp] [timestamp] NULL,
	[ModifyOrderItemAID] [int] NULL,
	[ModifyOrderItemLID] [smallint] NULL,
	[IsDeleted] [bit] NOT NULL,
	[ApplySelectionDiscount] [bit] NOT NULL,
	[CommissionUserAID] [int] NULL,
	[CommissionUserLID] [smallint] NULL,
	[ItemType] [tinyint] NOT NULL,
	[RefundReasonAID] [smallint] NULL,
	[RefundReasonLID] [smallint] NULL,
	[LineDiscount] [smallmoney] NOT NULL,
	[LineCost] [money] NOT NULL,
	[LoyaltyPoints] [smallint] NULL,
	[Course] [tinyint] NOT NULL,
	[Printed] [bit] NOT NULL,
	[Upgraded] [bit] NOT NULL,
	[SalesButtonAID] [int] NULL,
	[SalesButtonLID] [smallint] NULL,
	[IsNonMenu] [bit] NOT NULL,
	[VirtualProduct] [bit] NOT NULL,
	[IsSetMenuMaster] [bit] NULL,
	[SetMenuIndex] [tinyint] NULL,
	[IsSent] [bit] NOT NULL,
	[StationAID] [smallint] NULL,
	[StationLID] [smallint] NULL,
	[CreationTime] [datetime] NULL,
	[RecordAdded] [bit] NULL,
	[RecordEdited] [bit] NULL,
	[LocationsIdString] [varchar](4000) NULL,
	[PendingGiftCard] [bit] NOT NULL,
	[NoServiceChargeItem] [bit] NOT NULL,
	[SeatNum] [tinyint] NULL,
	[PendingGifCardCode] [nvarchar](50) NULL,
	[ShowModifierQuantity] [bit] NOT NULL,
	[IsComboMeal] [bit] NOT NULL,
	[ItemCode] [nvarchar](10) NULL,
	[ProductVariantAID] [int] NULL,
	[ProductVariantLID] [smallint] NULL,
	[IsPending] [bit] NOT NULL,
	[IsSubModifier] [bit] NOT NULL,
	[SizeFollowedParent] [bit] NOT NULL,
 CONSTRAINT [PK_OrderItems_1] PRIMARY KEY CLUSTERED 
(
	[OrderItemAID] ASC,
	[OrderItemLID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[OrderItemsAltTexts]    Script Date: 29/05/2019 10:55:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[OrderItemsAltTexts](
	[OrderItemAID] [int] NOT NULL,
	[OrderItemLID] [smallint] NOT NULL,
	[ReceiptAltText] [nvarchar](100) NULL,
	[KitchenAtlText] [nvarchar](100) NULL,
	[AppendReceiptAltText] [bit] NOT NULL,
	[RecordAdded] [bit] NULL,
	[RecordEdited] [bit] NULL,
	[LocationsIdString] [varchar](4000) NULL,
	[AppendKitchenAltText] [bit] NOT NULL,
 CONSTRAINT [PK_OrderItemsAltTexts] PRIMARY KEY CLUSTERED 
(
	[OrderItemAID] ASC,
	[OrderItemLID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[OrderItemSpecs]    Script Date: 29/05/2019 10:55:56 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[OrderItemSpecs](
	[OrderItemAID] [int] NOT NULL,
	[OrderItemLID] [smallint] NOT NULL,
	[SizeAID] [int] NULL,
	[SizeLID] [smallint] NULL,
	[Message] [nvarchar](2000) NULL,
	[Comments] [nvarchar](2000) NULL,
	[Occassion] [nvarchar](50) NULL,
	[Image] [image] NULL,
	[RecordAdded] [bit] NULL,
	[RecordEdited] [bit] NULL,
	[LocationsIdString] [varchar](4000) NULL,
 CONSTRAINT [PK_OrderItemSpecs_1] PRIMARY KEY CLUSTERED 
(
	[OrderItemAID] ASC,
	[OrderItemLID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[OrderlordItems]    Script Date: 29/05/2019 10:55:56 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[OrderlordItems](
	[OrderAID] [int] NOT NULL,
	[OrderLID] [smallint] NOT NULL,
	[TrackerHash] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_OrderlordItems] PRIMARY KEY CLUSTERED 
(
	[OrderAID] ASC,
	[OrderLID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Orders]    Script Date: 29/05/2019 10:55:56 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Orders](
	[OrderAID] [int] IDENTITY(1,1) NOT NULL,
	[OrderLID] [smallint] NOT NULL,
	[LocationID] [smallint] NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
	[OrderTypeAID] [int] NOT NULL,
	[OrderTypeLID] [smallint] NOT NULL,
	[TableID] [int] NULL,
	[CreationStationGroupAID] [smallint] NULL,
	[CreationStationGroupLID] [smallint] NULL,
	[CreationStationAID] [smallint] NOT NULL,
	[CreationStationLID] [smallint] NOT NULL,
	[CreationUserAID] [int] NOT NULL,
	[CreationUserLID] [smallint] NOT NULL,
	[CustomerAID] [int] NULL,
	[CustomerLID] [smallint] NULL,
	[CreationTime] [datetime] NOT NULL,
	[PaymentStationAID] [smallint] NULL,
	[PaymentStationLID] [smallint] NULL,
	[PaymentUserAID] [int] NULL,
	[PaymentUserLID] [smallint] NULL,
	[PaymentTime] [datetime] NULL,
	[DiscountType] [tinyint] NOT NULL,
	[DiscountPercent] [tinyint] NULL,
	[DiscountReasonAID] [smallint] NULL,
	[DiscountReasonLID] [smallint] NULL,
	[DiscountAmount] [money] NULL,
	[Amount] [money] NOT NULL,
	[OrderState] [tinyint] NOT NULL,
	[BillingDate] [smalldatetime] NULL,
	[Shift] [tinyint] NOT NULL,
	[ServiceChargePercent] [decimal](9, 2) NOT NULL,
	[Service] [smallmoney] NOT NULL,
	[CheckedOut] [bit] NOT NULL,
	[TimeStamp] [timestamp] NULL,
	[Covers] [smallint] NOT NULL,
	[ChargePerCover] [smallmoney] NOT NULL,
	[IsDeleted] [bit] NOT NULL,
	[NextCourseToPrint] [tinyint] NOT NULL,
	[EodCategoryAID] [int] NULL,
	[EodCategoryLID] [smallint] NULL,
	[LocalToBaseCurrencyRate] [decimal](9, 5) NOT NULL,
	[BillPrinted] [bit] NOT NULL,
	[DiscountPercentScaled] [bit] NOT NULL,
	[BookingRoomAID] [int] NULL,
	[BookingRoomLID] [smallint] NULL,
	[RecordAdded] [bit] NULL,
	[RecordEdited] [bit] NULL,
	[LocationsIdString] [varchar](4000) NULL,
	[QrCode] [varchar](10) NULL,
	[ServiceVatRate] [decimal](9, 3) NOT NULL,
	[ServiceVatCode] [nchar](2) NOT NULL,
	[IsAccountPayment] [bit] NULL,
	[SentCourseMask] [int] NULL,
	[ApplyServiceBeforeDiscount] [bit] NOT NULL,
	[PrintBillCount] [tinyint] NOT NULL,
	[OnlyPrintWithCourses] [bit] NOT NULL,
	[IsDepositPayment] [bit] NOT NULL,
	[IsTraining] [bit] NOT NULL,
	[ResDiaryBookingID] [int] NULL,
	[IsGiftCardPayment] [bit] NOT NULL,
	[ItemsHeldOrDefered] [bit] NOT NULL,
	[PagerNum] [smallint] NULL,
	[CoversChildren] [smallint] NOT NULL,
	[IsPending] [bit] NOT NULL,
 CONSTRAINT [PK_Orders] PRIMARY KEY CLUSTERED 
(
	[OrderAID] ASC,
	[OrderLID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[OrdersOffers]    Script Date: 29/05/2019 10:55:57 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[OrdersOffers](
	[OrderAID] [int] NOT NULL,
	[OrderLID] [smallint] NOT NULL,
	[DiscountRuleAID] [int] NOT NULL,
	[DiscountRuleLID] [smallint] NOT NULL,
	[Saving] [money] NOT NULL,
	[RecordAdded] [bit] NULL,
	[RecordEdited] [bit] NULL,
	[LocationsIdString] [varchar](4000) NULL,
 CONSTRAINT [PK_OrdersOffers] PRIMARY KEY CLUSTERED 
(
	[OrderAID] ASC,
	[OrderLID] ASC,
	[DiscountRuleAID] ASC,
	[DiscountRuleLID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[OrderTypeDisabledSalesButtons]    Script Date: 29/05/2019 10:55:57 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[OrderTypeDisabledSalesButtons](
	[OrderTypeAID] [int] NOT NULL,
	[OrderTypeLID] [smallint] NOT NULL,
	[SalesButtonAID] [int] NOT NULL,
	[SalesButtonLID] [smallint] NOT NULL,
	[TimeStamp] [timestamp] NULL,
	[RecordAdded] [bit] NULL,
	[RecordEdited] [bit] NULL,
	[LocationsIdString] [varchar](4000) NULL,
 CONSTRAINT [PK_OrderTypeDisabledSalesButtons] PRIMARY KEY CLUSTERED 
(
	[OrderTypeAID] ASC,
	[OrderTypeLID] ASC,
	[SalesButtonAID] ASC,
	[SalesButtonLID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[OrderTypes]    Script Date: 29/05/2019 10:55:57 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[OrderTypes](
	[OrderTypeAID] [int] IDENTITY(1,1) NOT NULL,
	[OrderTypeLID] [smallint] NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[OrderPrefix] [nvarchar](50) NULL,
	[IsOnTheFly] [bit] NOT NULL,
	[AllowAssignTo] [bit] NOT NULL,
	[ApplyVatToAll] [bit] NOT NULL,
	[ApplyService] [bit] NOT NULL,
	[ApplyCover] [bit] NOT NULL,
	[IsDefault] [bit] NOT NULL,
	[IsAcrchived] [bit] NOT NULL,
	[Position] [int] NULL,
	[IsTakeaway] [bit] NOT NULL,
	[IsDelivery] [bit] NOT NULL,
	[PricePointAID] [smallint] NOT NULL,
	[PricePointLID] [smallint] NOT NULL,
	[EodName] [nvarchar](50) NOT NULL,
	[NominalCode] [int] NULL,
	[AccountPayment] [bit] NOT NULL,
	[VatSchemeAID] [int] NULL,
	[VatSchemeLID] [smallint] NULL,
	[IsAccommodation] [bit] NOT NULL,
	[RecordAdded] [bit] NULL,
	[RecordEdited] [bit] NULL,
	[LocationsIdString] [varchar](4000) NULL,
	[IsTable] [bit] NOT NULL,
	[PhoneAssign] [bit] NOT NULL,
	[KitchenScreenColour] [int] NOT NULL,
	[DepositPayment] [bit] NOT NULL,
	[ConvertOnTheFlyToThis] [bit] NOT NULL,
	[UseTakeawayIngredients] [bit] NOT NULL,
	[EposHubProviderID] [int] NULL,
	[DeliveryCharge] [money] NULL,
	[DeliveryFreeOver] [money] NULL,
	[ScreenPriority] [smallint] NOT NULL,
 CONSTRAINT [PK_OrderTypes_1] PRIMARY KEY CLUSTERED 
(
	[OrderTypeAID] ASC,
	[OrderTypeLID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ProductOptions]    Script Date: 29/05/2019 10:55:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ProductOptions](
	[ProductOptionAID] [int] IDENTITY(1,1) NOT NULL,
	[ProductOptionLID] [smallint] NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
	[RecordAdded] [bit] NULL,
	[RecordEdited] [bit] NULL,
	[LocationsIdString] [varchar](4000) NULL,
 CONSTRAINT [PK_ProductOptions] PRIMARY KEY CLUSTERED 
(
	[ProductOptionAID] ASC,
	[ProductOptionLID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ProductPrices]    Script Date: 29/05/2019 10:55:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ProductPrices](
	[ProductAID] [int] NOT NULL,
	[ProductLID] [smallint] NOT NULL,
	[PricePointAID] [smallint] NOT NULL,
	[PricePointLID] [smallint] NOT NULL,
	[Amount] [smallmoney] NOT NULL,
	[TimeStamp] [timestamp] NULL,
	[RecordAdded] [bit] NULL,
	[RecordEdited] [bit] NULL,
	[LocationsIdString] [varchar](4000) NULL,
 CONSTRAINT [PK_ProductPrices] PRIMARY KEY CLUSTERED 
(
	[ProductAID] ASC,
	[ProductLID] ASC,
	[PricePointAID] ASC,
	[PricePointLID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Products]    Script Date: 29/05/2019 10:55:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Products](
	[ProductAID] [int] IDENTITY(1,1) NOT NULL,
	[ProductLID] [smallint] NOT NULL,
	[SubcategoryAID] [int] NOT NULL,
	[SubcategoryLID] [smallint] NOT NULL,
	[ProductType] [tinyint] NOT NULL,
	[ProductCode] [nvarchar](50) NULL,
	[Name] [nvarchar](100) NOT NULL,
	[AlternativeName] [nvarchar](100) NULL,
	[Code] [nvarchar](50) NULL,
	[VATCode] [nchar](2) NOT NULL,
	[VAT] [bit] NOT NULL,
	[ScaleItem] [bit] NOT NULL,
	[CookingTimeMinutes] [smallint] NULL,
	[TimeStamp] [timestamp] NULL,
	[IsArchived] [bit] NOT NULL,
	[IsModifyer] [bit] NOT NULL,
	[Recipe] [nvarchar](4000) NULL,
	[Duration] [smallint] NULL,
	[PreTime] [smallint] NULL,
	[CleanupTime] [smallint] NULL,
	[ServiceDescription] [nvarchar](4000) NULL,
	[ServiceRemarks] [nvarchar](4000) NULL,
	[LoyaltyPoints] [smallint] NULL,
	[ServicePackageCount] [tinyint] NULL,
	[ServicePackageMaxPrice] [money] NULL,
	[ServicePackageServiceAID] [int] NULL,
	[ServicePackageServiceLID] [smallint] NULL,
	[DefaultPrice1] [smallmoney] NULL,
	[DefaultPrice2] [smallmoney] NULL,
	[DefaultPrice3] [smallmoney] NULL,
	[DefaultPrice4] [smallmoney] NULL,
	[DefaultPrice5] [smallmoney] NULL,
	[DefaultPrice6] [smallmoney] NULL,
	[Manufactured] [bit] NOT NULL,
	[WarningMessage] [nvarchar](50) NULL,
	[PLU] [nvarchar](50) NULL,
	[VirtualProduct] [bit] NOT NULL,
	[IsLarge] [bit] NOT NULL,
	[ProductFamilyAID] [int] NULL,
	[ProductFamilyLID] [smallint] NULL,
	[LabelText] [nvarchar](500) NULL,
	[RecordAdded] [bit] NULL,
	[RecordEdited] [bit] NULL,
	[LocationsIdString] [varchar](4000) NULL,
	[ShowOnWebsite] [bit] NOT NULL,
	[WebDescription] [nvarchar](4000) NULL,
	[ShelfLifeDays] [smallint] NULL,
	[FeaturedProduct] [bit] NOT NULL,
	[OurPicksProduct] [bit] NOT NULL,
	[IsVegetarian] [bit] NOT NULL,
	[CannotBuyOnWebsite] [bit] NOT NULL,
	[BestSeller] [bit] NOT NULL,
	[NoServiceChargeItem] [bit] NOT NULL,
	[ProductLableLine1] [nvarchar](50) NULL,
	[ProductLableLine2] [nvarchar](50) NULL,
	[ProductLableLine3] [nvarchar](50) NULL,
	[AutoAddToOrders] [bit] NOT NULL,
	[ScreenProductGroupAID] [int] NULL,
	[ScreenProductGroupLID] [smallint] NULL,
	[AltReceiptText] [nvarchar](100) NULL,
	[AltKitchenText] [nvarchar](100) NULL,
	[AppendAltReceiptText] [bit] NOT NULL,
	[AppendAltKitchenText] [bit] NOT NULL,
	[Cost] [money] NULL,
	[HideOnScreens] [bit] NOT NULL,
 CONSTRAINT [PK_Products] PRIMARY KEY CLUSTERED 
(
	[ProductAID] ASC,
	[ProductLID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Stock]    Script Date: 29/05/2019 10:55:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Stock](
	[StockAID] [int] IDENTITY(1,1) NOT NULL,
	[StockLID] [smallint] NOT NULL,
	[LocationID] [int] NOT NULL,
	[StockItemAID] [int] NOT NULL,
	[StockItemLID] [smallint] NOT NULL,
	[TransactionTime] [datetime] NOT NULL,
	[StockIn] [bit] NOT NULL,
	[Reason] [tinyint] NOT NULL,
	[Quantity] [decimal](9, 3) NOT NULL,
	[Price] [smallmoney] NOT NULL,
	[QuantityLeft] [decimal](9, 3) NOT NULL,
	[RecordAdded] [bit] NULL,
	[RecordEdited] [bit] NULL,
	[LocationsIdString] [varchar](4000) NULL,
	[RevenueCentreAID] [int] NULL,
	[RevenueCentreLID] [smallint] NULL,
	[OrderItemAID] [int] NULL,
	[OrderItemLID] [smallint] NULL,
	[ProductAID] [int] NULL,
	[ProductLID] [smallint] NULL,
 CONSTRAINT [PK_Stock] PRIMARY KEY CLUSTERED 
(
	[StockAID] ASC,
	[StockLID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[StockCategories]    Script Date: 29/05/2019 10:55:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[StockCategories](
	[StockCategroyAID] [int] IDENTITY(1,1) NOT NULL,
	[StockCategoryLID] [smallint] NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[TimeStamp] [timestamp] NULL,
	[RecordAdded] [bit] NULL,
	[RecordEdited] [bit] NULL,
	[LocationsIdString] [varchar](4000) NULL,
	[NominalCode] [int] NULL,
 CONSTRAINT [PK_StockCategories] PRIMARY KEY CLUSTERED 
(
	[StockCategroyAID] ASC,
	[StockCategoryLID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[StockItemBatches]    Script Date: 29/05/2019 10:56:00 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[StockItemBatches](
	[StockItemBatchAID] [int] IDENTITY(1,1) NOT NULL,
	[StockItemBatchLID] [smallint] NOT NULL,
	[StockItemAID] [int] NULL,
	[StockItemLID] [smallint] NULL,
	[BatchNumber] [nvarchar](50) NOT NULL,
	[GeneratedAtLocationID] [smallint] NOT NULL,
	[GeneratedTime] [datetime] NOT NULL,
	[GeneratedByUserAID] [int] NOT NULL,
	[GeneratedByUserLID] [smallint] NOT NULL,
	[StockOrderItemAID] [int] NULL,
	[StockOrderItemLID] [smallint] NULL,
	[Quantity] [decimal](18, 9) NOT NULL,
	[QuantityLeft] [decimal](18, 9) NOT NULL,
	[ProductAID] [int] NULL,
	[ProductLID] [smallint] NULL,
	[StationAID] [smallint] NULL,
	[StationLID] [smallint] NULL,
	[OrderItemAID] [int] NULL,
	[OrderItemLID] [smallint] NULL,
 CONSTRAINT [PK_StockItemBatches] PRIMARY KEY CLUSTERED 
(
	[StockItemBatchAID] ASC,
	[StockItemBatchLID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[StockItemIngredients]    Script Date: 29/05/2019 10:56:00 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[StockItemIngredients](
	[StockItemAID] [int] NOT NULL,
	[StockItemLID] [smallint] NOT NULL,
	[IngredientStockItemAID] [int] NOT NULL,
	[IngredientStockItemLID] [smallint] NOT NULL,
	[Quantity] [decimal](9, 3) NOT NULL,
	[UnitAID] [int] NOT NULL,
	[UnitLID] [smallint] NOT NULL,
	[TimeStamp] [timestamp] NULL,
	[RecordAdded] [bit] NULL,
	[RecordEdited] [bit] NULL,
	[LocationsIdString] [varchar](4000) NULL,
	[TakeawayOnly] [bit] NOT NULL,
 CONSTRAINT [PK_StockItemIngredients] PRIMARY KEY CLUSTERED 
(
	[StockItemAID] ASC,
	[StockItemLID] ASC,
	[IngredientStockItemAID] ASC,
	[IngredientStockItemLID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[StockItems]    Script Date: 29/05/2019 10:56:00 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[StockItems](
	[StockItemAID] [int] IDENTITY(1,1) NOT NULL,
	[StockItemLID] [smallint] NOT NULL,
	[ProductAID] [int] NULL,
	[ProductLID] [smallint] NULL,
	[Name] [nvarchar](100) NOT NULL,
	[VATCode] [nchar](2) NOT NULL,
	[VAT] [bit] NOT NULL,
	[TimeStamp] [timestamp] NULL,
	[IsArchived] [bit] NOT NULL,
	[CanBeIngredient] [bit] NOT NULL,
	[ItemDescription] [nvarchar](2000) NULL,
	[StockTakeHigherTolerance] [decimal](9, 3) NOT NULL,
	[StockTakeLowerTolerance] [numeric](9, 3) NOT NULL,
	[StockTakeRoundDown] [bit] NOT NULL,
	[ShelfLife] [nvarchar](30) NULL,
	[BaseUnitAID] [int] NOT NULL,
	[BaseUnitLID] [smallint] NOT NULL,
	[SalesUnitAID] [int] NOT NULL,
	[SalesUnitLID] [smallint] NOT NULL,
	[WieghtPerBaseUnit] [decimal](9, 3) NULL,
	[IsCombination] [bit] NOT NULL,
	[CombineOnSale] [bit] NOT NULL,
	[StockCategoryAID] [int] NULL,
	[StockCategoryLID] [smallint] NULL,
	[ECCode] [int] NULL,
	[NominalCode] [int] NULL,
	[RecordAdded] [bit] NULL,
	[RecordEdited] [bit] NULL,
	[LocationsIdString] [varchar](4000) NULL,
	[StockCode] [nvarchar](40) NULL,
	[OnlyManufacturedInProduction] [bit] NOT NULL,
	[StockItemType] [tinyint] NOT NULL,
	[GenerateBatchNumbers] [bit] NOT NULL,
	[CostChanged] [bit] NOT NULL,
	[DepletionType] [tinyint] NOT NULL,
	[QuantityPerDepletionUnit] [decimal](18, 9) NULL,
	[HideInStockTake] [bit] NOT NULL,
	[ChefDirectID] [nvarchar](100) NULL,
 CONSTRAINT [PK_StockItems] PRIMARY KEY CLUSTERED 
(
	[StockItemAID] ASC,
	[StockItemLID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[StockItemSize]    Script Date: 29/05/2019 10:56:01 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[StockItemSize](
	[StockItemAID] [int] NOT NULL,
	[StockItemLID] [smallint] NOT NULL,
	[SizeAID] [int] NOT NULL,
	[SizeLID] [smallint] NOT NULL,
	[RecordAdded] [bit] NULL,
	[RecordEdited] [bit] NULL,
	[LocationsIdString] [varchar](4000) NULL,
 CONSTRAINT [PK_StockItemSize] PRIMARY KEY CLUSTERED 
(
	[StockItemAID] ASC,
	[StockItemLID] ASC,
	[SizeAID] ASC,
	[SizeLID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[StockItemSizeIngredients]    Script Date: 29/05/2019 10:56:01 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[StockItemSizeIngredients](
	[StockItemAID] [int] NOT NULL,
	[StockItemLID] [smallint] NOT NULL,
	[IngredientStockItemAID] [int] NOT NULL,
	[IngredientStockItemLID] [smallint] NOT NULL,
	[SizeAID] [int] NOT NULL,
	[SizeLID] [smallint] NOT NULL,
	[Quantity] [decimal](9, 3) NOT NULL,
	[UnitAID] [int] NOT NULL,
	[UnitLID] [smallint] NOT NULL,
	[RecordAdded] [bit] NULL,
	[RecordEdited] [bit] NULL,
	[LocationsIdString] [varchar](4000) NULL,
	[TakeawayOnly] [bit] NOT NULL,
 CONSTRAINT [PK_StockItemSizeIngredients] PRIMARY KEY CLUSTERED 
(
	[StockItemAID] ASC,
	[StockItemLID] ASC,
	[IngredientStockItemAID] ASC,
	[IngredientStockItemLID] ASC,
	[SizeAID] ASC,
	[SizeLID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[StockItemSuppliers]    Script Date: 29/05/2019 10:56:01 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[StockItemSuppliers](
	[StockItemAID] [int] NOT NULL,
	[StockItemLID] [smallint] NOT NULL,
	[SupplierAID] [int] NOT NULL,
	[SupplierLID] [smallint] NOT NULL,
	[DefaultPricePerUnit] [smallmoney] NULL,
	[MaxOrderQuantity] [decimal](9, 3) NULL,
	[MinOrderQuantity] [decimal](9, 3) NULL,
	[TimeStamp] [timestamp] NULL,
	[SupplyUnitAID] [int] NOT NULL,
	[SupplyUnitLID] [smallint] NOT NULL,
	[SuppliersStockCode] [nvarchar](20) NULL,
	[VATCode] [nchar](2) NOT NULL,
	[RecordAdded] [bit] NULL,
	[RecordEdited] [bit] NULL,
	[LocationsIdString] [varchar](4000) NULL,
 CONSTRAINT [PK_StockItemSuppliers] PRIMARY KEY CLUSTERED 
(
	[StockItemAID] ASC,
	[StockItemLID] ASC,
	[SupplierAID] ASC,
	[SupplierLID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[StockOrderItems]    Script Date: 29/05/2019 10:56:02 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[StockOrderItems](
	[StockOrderItemAID] [int] IDENTITY(1,1) NOT NULL,
	[StockOrderItemLID] [smallint] NOT NULL,
	[StockOrderAID] [int] NOT NULL,
	[StockOrderLID] [smallint] NOT NULL,
	[StockItemAID] [int] NULL,
	[StockItemLID] [smallint] NULL,
	[Name] [nvarchar](100) NOT NULL,
	[InitialQuantity] [decimal](9, 3) NOT NULL,
	[ActualQuantity] [decimal](9, 3) NOT NULL,
	[PricePerUnit] [smallmoney] NOT NULL,
	[TimeStamp] [timestamp] NULL,
	[VATCode] [nchar](2) NOT NULL,
	[VATAmount] [decimal](19, 11) NOT NULL,
	[PartReceived] [bit] NOT NULL,
	[UnitAID] [int] NOT NULL,
	[UnitLID] [smallint] NOT NULL,
	[RecordAdded] [bit] NULL,
	[RecordEdited] [bit] NULL,
	[LocationsIdString] [varchar](4000) NULL,
 CONSTRAINT [PK_StockOrderItems_1] PRIMARY KEY CLUSTERED 
(
	[StockOrderItemAID] ASC,
	[StockOrderItemLID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[StockOrderNumbers]    Script Date: 29/05/2019 10:56:02 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[StockOrderNumbers](
	[LocationID] [smallint] NOT NULL,
	[LastStockOrderNum] [int] NOT NULL,
	[RecordAdded] [bit] NULL,
	[RecordEdited] [bit] NULL,
	[LocationsIdString] [varchar](4000) NULL,
 CONSTRAINT [PK_StockOrderNumbers] PRIMARY KEY CLUSTERED 
(
	[LocationID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[StockOrders]    Script Date: 29/05/2019 10:56:02 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[StockOrders](
	[StockOrderAID] [int] IDENTITY(1,1) NOT NULL,
	[StockOrderLID] [smallint] NOT NULL,
	[OrderNumber] [nvarchar](50) NOT NULL,
	[DeliveryLocationID] [smallint] NOT NULL,
	[SupplierAID] [int] NOT NULL,
	[SupplierLID] [smallint] NOT NULL,
	[OrderDate] [datetime] NOT NULL,
	[StockOrderState] [tinyint] NOT NULL,
	[CreationUserAID] [int] NOT NULL,
	[CreationUserLID] [smallint] NOT NULL,
	[DispatchedTime] [datetime] NULL,
	[ReceivedTime] [datetime] NULL,
	[ReceivedUserAID] [int] NULL,
	[ReceivedUserLID] [smallint] NULL,
	[VATStandardRate] [decimal](9, 2) NOT NULL,
	[Amount] [money] NOT NULL,
	[TimeStamp] [timestamp] NULL,
	[PickerUserAID] [int] NULL,
	[PickerUserLID] [smallint] NULL,
	[Weight] [int] NULL,
	[VanAID] [int] NULL,
	[VanLID] [smallint] NULL,
	[DriverUserAID] [int] NULL,
	[DriverUserLID] [smallint] NULL,
	[OrderToBaseCurrencyRate] [decimal](9, 5) NOT NULL,
	[Recurrence] [tinyint] NOT NULL,
	[SendToSage] [bit] NOT NULL,
	[InSage] [bit] NOT NULL,
	[RecordAdded] [bit] NULL,
	[RecordEdited] [bit] NULL,
	[LocationsIdString] [varchar](4000) NULL,
	[Emailed] [bit] NOT NULL,
	[IsSubmitted] [bit] NOT NULL,
	[DispatchedByAID] [int] NULL,
	[DispatchedByLID] [smallint] NULL,
	[ThirdPartyCustomerAID] [int] NULL,
	[ThirdPartyCustomerLID] [smallint] NULL,
	[OnlineOrderState] [tinyint] NOT NULL,
	[SubmittedTime] [datetime] NULL,
	[SubmittedByAID] [int] NULL,
	[SubmittedByLID] [smallint] NULL,
	[InvoiceNumber] [nvarchar](50) NULL,
	[InvoiceDate] [datetime] NULL,
	[SendToBestway] [bit] NOT NULL,
	[SentToBestway] [bit] NOT NULL,
	[SendChefDirectOrder] [bit] NOT NULL,
	[SentChefDirectOrder] [bit] NOT NULL,
	[DeliveryCharge] [money] NOT NULL,
	[RequestedDeliveryDate] [datetime] NULL,
 CONSTRAINT [PK_StockOrders_1] PRIMARY KEY CLUSTERED 
(
	[StockOrderAID] ASC,
	[StockOrderLID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[StockTake]    Script Date: 29/05/2019 10:56:03 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[StockTake](
	[StockTakeAID] [int] IDENTITY(1,1) NOT NULL,
	[StockTakeLID] [smallint] NOT NULL,
	[LocationID] [smallint] NOT NULL,
	[Time] [datetime] NOT NULL,
	[UserAID] [int] NOT NULL,
	[UserLID] [smallint] NOT NULL,
	[TimeStamp] [timestamp] NULL,
	[AppliedToStock] [bit] NOT NULL,
	[HeldUntilLocal] [bit] NOT NULL,
	[IsSnapshot] [bit] NOT NULL,
	[RecordAdded] [bit] NULL,
	[RecordEdited] [bit] NULL,
	[LocationsIdString] [varchar](4000) NULL,
	[RevenueCentreAID] [int] NULL,
	[RevenueCentreLID] [smallint] NULL,
 CONSTRAINT [PK_StockTake] PRIMARY KEY CLUSTERED 
(
	[StockTakeAID] ASC,
	[StockTakeLID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[StockTakeItem]    Script Date: 29/05/2019 10:56:03 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[StockTakeItem](
	[StockTakeItemAID] [int] IDENTITY(1,1) NOT NULL,
	[StockTakeItemLID] [smallint] NOT NULL,
	[StockTakeAID] [int] NOT NULL,
	[StockTakeLID] [smallint] NOT NULL,
	[StockItemAID] [int] NOT NULL,
	[StockItemLID] [smallint] NOT NULL,
	[ExpectedStock] [decimal](9, 3) NOT NULL,
	[CountedStock] [decimal](9, 3) NULL,
	[TimeStamp] [timestamp] NULL,
	[Applied] [bit] NOT NULL,
	[StockUnitCost] [money] NOT NULL,
	[RecordAdded] [bit] NULL,
	[RecordEdited] [bit] NULL,
	[LocationsIdString] [varchar](4000) NULL,
	[CachedValue] [nvarchar](30) NULL,
	[OutOfRange] [bit] NOT NULL,
 CONSTRAINT [PK_StockTakeItem] PRIMARY KEY CLUSTERED 
(
	[StockTakeItemAID] ASC,
	[StockTakeItemLID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[StockTransfers]    Script Date: 29/05/2019 10:56:03 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[StockTransfers](
	[StockTransferAID] [int] IDENTITY(1,1) NOT NULL,
	[StockTransferLID] [smallint] NOT NULL,
	[StockItemAID] [int] NOT NULL,
	[StockItemLID] [smallint] NOT NULL,
	[SendingLocationID] [smallint] NOT NULL,
	[SendingUserAID] [int] NOT NULL,
	[SendingUserLID] [smallint] NOT NULL,
	[SendDate] [datetime] NOT NULL,
	[ReceivingLocationID] [smallint] NOT NULL,
	[Received] [bit] NOT NULL,
	[Cancelled] [bit] NOT NULL,
	[ReceiveOrCancelUserAID] [int] NULL,
	[ReceiveOrCancelUserLID] [smallint] NULL,
	[ReceiveOrCancelDate] [datetime] NULL,
	[QuantiyInBaseUnits] [decimal](18, 8) NOT NULL,
	[UnitCost] [money] NOT NULL,
	[CheckoutHeldUntilLocal] [bit] NOT NULL,
	[RecordAdded] [bit] NULL,
	[RecordEdited] [bit] NULL,
	[LocationsIdString] [varchar](4000) NULL,
	[SendingRevenueCentreAID] [int] NULL,
	[SendingRevenueCentreLID] [smallint] NULL,
	[ReceivingRevenueCentreAID] [int] NULL,
	[ReceivingRevenueCentreLID] [smallint] NULL,
 CONSTRAINT [PK_StockTransfers] PRIMARY KEY CLUSTERED 
(
	[StockTransferAID] ASC,
	[StockTransferLID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[WebOrderDetails]    Script Date: 29/05/2019 10:56:04 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[WebOrderDetails](
	[OrderAID] [int] NOT NULL,
	[OrderLID] [smallint] NOT NULL,
	[JSON] [nvarchar](max) NULL,
	[Viewed] [bit] NOT NULL,
	[RecordAdded] [bit] NULL,
	[RecordEdited] [bit] NULL,
	[LocationsIdString] [varchar](4000) NULL,
	[TimeLimitExtension] [int] NULL,
	[IsAppOrder] [bit] NOT NULL,
	[NumAdultAllYouCanEat] [int] NOT NULL,
	[NumChildAllYouCanEat] [int] NOT NULL,
	[NumALaCarte] [int] NOT NULL,
	[BillRequested] [bit] NOT NULL,
	[WaiterCalled] [bit] NOT NULL,
	[ItemsPending] [bit] NOT NULL,
 CONSTRAINT [PK_WebOrderDetails] PRIMARY KEY CLUSTERED 
(
	[OrderAID] ASC,
	[OrderLID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
ALTER TABLE [dbo].[OrderBookingInfo] ADD  DEFAULT ((1)) FOR [RecordAdded]
GO
ALTER TABLE [dbo].[OrderBookingInfo] ADD  DEFAULT ((0)) FOR [RecordEdited]
GO
ALTER TABLE [dbo].[OrderItems] ADD  CONSTRAINT [DF_OrderItems_OrderItemLID]  DEFAULT ((1)) FOR [OrderItemLID]
GO
ALTER TABLE [dbo].[OrderItems] ADD  CONSTRAINT [DF_OrderItems_OrderLID]  DEFAULT ((1)) FOR [OrderLID]
GO
ALTER TABLE [dbo].[OrderItems] ADD  CONSTRAINT [DF_OrderItems_ProductLID]  DEFAULT ((1)) FOR [ProductLID]
GO
ALTER TABLE [dbo].[OrderItems] ADD  CONSTRAINT [DF_OrderItems_SeatLID]  DEFAULT ((1)) FOR [SeatLID]
GO
ALTER TABLE [dbo].[OrderItems] ADD  CONSTRAINT [DF_OrderItems_Quantity]  DEFAULT ((1)) FOR [Quantity]
GO
ALTER TABLE [dbo].[OrderItems] ADD  CONSTRAINT [DF_OrderItems_ApplyVAT]  DEFAULT ((1)) FOR [VAT]
GO
ALTER TABLE [dbo].[OrderItems] ADD  CONSTRAINT [DF_OrderItems_VATCode]  DEFAULT (N'S') FOR [VATCode]
GO
ALTER TABLE [dbo].[OrderItems] ADD  CONSTRAINT [DF_OrderItems_ProductType]  DEFAULT ((0)) FOR [ProductType]
GO
ALTER TABLE [dbo].[OrderItems] ADD  CONSTRAINT [DF_OrderItems_UserLID]  DEFAULT ((1)) FOR [UserLID]
GO
ALTER TABLE [dbo].[OrderItems] ADD  CONSTRAINT [DF_OrderItems_IsDeleted]  DEFAULT ((0)) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[OrderItems] ADD  CONSTRAINT [DF_OrderItems_ApplyDiscount]  DEFAULT ((0)) FOR [ApplySelectionDiscount]
GO
ALTER TABLE [dbo].[OrderItems] ADD  CONSTRAINT [DF_OrderItems_CommissionUserLID]  DEFAULT ((1)) FOR [CommissionUserLID]
GO
ALTER TABLE [dbo].[OrderItems] ADD  CONSTRAINT [DF_OrderItems_ItemType]  DEFAULT ((0)) FOR [ItemType]
GO
ALTER TABLE [dbo].[OrderItems] ADD  CONSTRAINT [DF_OrderItems_RefindReasonLID]  DEFAULT ((1)) FOR [RefundReasonLID]
GO
ALTER TABLE [dbo].[OrderItems] ADD  CONSTRAINT [DF_OrderItems_LineDiscount]  DEFAULT ((0)) FOR [LineDiscount]
GO
ALTER TABLE [dbo].[OrderItems] ADD  CONSTRAINT [DF_OrderItems_Printed]  DEFAULT ((0)) FOR [Course]
GO
ALTER TABLE [dbo].[OrderItems] ADD  CONSTRAINT [DF_OrderItems_Printed_1]  DEFAULT ((0)) FOR [Printed]
GO
ALTER TABLE [dbo].[OrderItems] ADD  CONSTRAINT [DF_OrderItems_Upgraded]  DEFAULT ((0)) FOR [Upgraded]
GO
ALTER TABLE [dbo].[OrderItems] ADD  CONSTRAINT [DF_OrderItems_IsNonMenu]  DEFAULT ((0)) FOR [IsNonMenu]
GO
ALTER TABLE [dbo].[OrderItems] ADD  CONSTRAINT [DF_OrderItems_VirtualProduct]  DEFAULT ((0)) FOR [VirtualProduct]
GO
ALTER TABLE [dbo].[OrderItems] ADD  CONSTRAINT [DF_OrderItems_IsSent]  DEFAULT ((1)) FOR [IsSent]
GO
ALTER TABLE [dbo].[OrderItems] ADD  DEFAULT ((1)) FOR [RecordAdded]
GO
ALTER TABLE [dbo].[OrderItems] ADD  DEFAULT ((0)) FOR [RecordEdited]
GO
ALTER TABLE [dbo].[OrderItems] ADD  CONSTRAINT [DF_OrderItems_PendingGiftCard]  DEFAULT ((0)) FOR [PendingGiftCard]
GO
ALTER TABLE [dbo].[OrderItems] ADD  CONSTRAINT [DF_OrderItems_NoServiceChargeItem]  DEFAULT ((0)) FOR [NoServiceChargeItem]
GO
ALTER TABLE [dbo].[OrderItems] ADD  CONSTRAINT [DF_OrderItems_ShowModifierQuant]  DEFAULT ((0)) FOR [ShowModifierQuantity]
GO
ALTER TABLE [dbo].[OrderItems] ADD  CONSTRAINT [DF_OrderItems_IsMealDeal]  DEFAULT ((0)) FOR [IsComboMeal]
GO
ALTER TABLE [dbo].[OrderItems] ADD  CONSTRAINT [DF_OrderItems_IsPending]  DEFAULT ((0)) FOR [IsPending]
GO
ALTER TABLE [dbo].[OrderItems] ADD  CONSTRAINT [DF_OrderItems_IsSubModifier]  DEFAULT ((0)) FOR [IsSubModifier]
GO
ALTER TABLE [dbo].[OrderItems] ADD  CONSTRAINT [DF_OrderItems_SizeFollowedParent]  DEFAULT ((0)) FOR [SizeFollowedParent]
GO
ALTER TABLE [dbo].[OrderItemsAltTexts] ADD  DEFAULT ((1)) FOR [RecordAdded]
GO
ALTER TABLE [dbo].[OrderItemsAltTexts] ADD  DEFAULT ((0)) FOR [RecordEdited]
GO
ALTER TABLE [dbo].[OrderItemsAltTexts] ADD  CONSTRAINT [DF_OrderItemsAltTexts_AppendKitchenAltText]  DEFAULT ((0)) FOR [AppendKitchenAltText]
GO
ALTER TABLE [dbo].[OrderItemSpecs] ADD  DEFAULT ((1)) FOR [RecordAdded]
GO
ALTER TABLE [dbo].[OrderItemSpecs] ADD  DEFAULT ((0)) FOR [RecordEdited]
GO
ALTER TABLE [dbo].[Orders] ADD  CONSTRAINT [DF_Orders_OrderLID]  DEFAULT ((1)) FOR [OrderLID]
GO
ALTER TABLE [dbo].[Orders] ADD  CONSTRAINT [DF_Orders_LocationID]  DEFAULT ((1)) FOR [LocationID]
GO
ALTER TABLE [dbo].[Orders] ADD  CONSTRAINT [DF_Orders_OrderTypeLID]  DEFAULT ((1)) FOR [OrderTypeLID]
GO
ALTER TABLE [dbo].[Orders] ADD  CONSTRAINT [DF_Orders_CreationStationGroupLID]  DEFAULT ((1)) FOR [CreationStationGroupLID]
GO
ALTER TABLE [dbo].[Orders] ADD  CONSTRAINT [DF_Orders_CreationStationLID]  DEFAULT ((1)) FOR [CreationStationLID]
GO
ALTER TABLE [dbo].[Orders] ADD  CONSTRAINT [DF_Orders_CreationUserLID]  DEFAULT ((1)) FOR [CreationUserLID]
GO
ALTER TABLE [dbo].[Orders] ADD  CONSTRAINT [DF_Orders_CustomerLID]  DEFAULT ((1)) FOR [CustomerLID]
GO
ALTER TABLE [dbo].[Orders] ADD  CONSTRAINT [DF_Orders_PaymentStationLID]  DEFAULT ((1)) FOR [PaymentStationLID]
GO
ALTER TABLE [dbo].[Orders] ADD  CONSTRAINT [DF_Orders_PaymentUserLID]  DEFAULT ((1)) FOR [PaymentUserLID]
GO
ALTER TABLE [dbo].[Orders] ADD  CONSTRAINT [DF_Orders_DiscountType]  DEFAULT ((0)) FOR [DiscountType]
GO
ALTER TABLE [dbo].[Orders] ADD  CONSTRAINT [DF_Orders_DiscountReasonLID]  DEFAULT ((1)) FOR [DiscountReasonLID]
GO
ALTER TABLE [dbo].[Orders] ADD  CONSTRAINT [DF_Orders_Service]  DEFAULT ((0)) FOR [Service]
GO
ALTER TABLE [dbo].[Orders] ADD  CONSTRAINT [DF_Orders_CheckedOut]  DEFAULT ((0)) FOR [CheckedOut]
GO
ALTER TABLE [dbo].[Orders] ADD  CONSTRAINT [DF_Orders_Covers]  DEFAULT ((0)) FOR [Covers]
GO
ALTER TABLE [dbo].[Orders] ADD  CONSTRAINT [DF_Orders_ChargePerCover]  DEFAULT ((0)) FOR [ChargePerCover]
GO
ALTER TABLE [dbo].[Orders] ADD  CONSTRAINT [DF_Orders_IsDeleted]  DEFAULT ((0)) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[Orders] ADD  CONSTRAINT [DF_Orders_NextCourseToPrint]  DEFAULT ((1)) FOR [NextCourseToPrint]
GO
ALTER TABLE [dbo].[Orders] ADD  CONSTRAINT [DF_Orders_LocalToBaseCurrencyRate]  DEFAULT ((1)) FOR [LocalToBaseCurrencyRate]
GO
ALTER TABLE [dbo].[Orders] ADD  CONSTRAINT [DF_Orders_BillPrinted]  DEFAULT ((0)) FOR [BillPrinted]
GO
ALTER TABLE [dbo].[Orders] ADD  CONSTRAINT [DF_Orders_DiscountPercentScaled]  DEFAULT ((0)) FOR [DiscountPercentScaled]
GO
ALTER TABLE [dbo].[Orders] ADD  DEFAULT ((1)) FOR [RecordAdded]
GO
ALTER TABLE [dbo].[Orders] ADD  DEFAULT ((0)) FOR [RecordEdited]
GO
ALTER TABLE [dbo].[Orders] ADD  CONSTRAINT [DF_Orders_ServiceVatRate]  DEFAULT ((0)) FOR [ServiceVatRate]
GO
ALTER TABLE [dbo].[Orders] ADD  CONSTRAINT [DF_Orders_ServiceVatCode]  DEFAULT (N'T0') FOR [ServiceVatCode]
GO
ALTER TABLE [dbo].[Orders] ADD  CONSTRAINT [DF_Orders_IsAccountPayment]  DEFAULT ((0)) FOR [IsAccountPayment]
GO
ALTER TABLE [dbo].[Orders] ADD  CONSTRAINT [DF_Orders_ApplyServiceBeforeDiscount]  DEFAULT ((0)) FOR [ApplyServiceBeforeDiscount]
GO
ALTER TABLE [dbo].[Orders] ADD  CONSTRAINT [DF_Orders_BillPrintCount]  DEFAULT ((0)) FOR [PrintBillCount]
GO
ALTER TABLE [dbo].[Orders] ADD  CONSTRAINT [DF_Orders_OnlyPrintWithCourse]  DEFAULT ((0)) FOR [OnlyPrintWithCourses]
GO
ALTER TABLE [dbo].[Orders] ADD  CONSTRAINT [DF_Orders_IsDepositPayment]  DEFAULT ((0)) FOR [IsDepositPayment]
GO
ALTER TABLE [dbo].[Orders] ADD  CONSTRAINT [DF_Orders_IsTraining]  DEFAULT ((0)) FOR [IsTraining]
GO
ALTER TABLE [dbo].[Orders] ADD  CONSTRAINT [DF_Orders_IsGiftCardPayment]  DEFAULT ((0)) FOR [IsGiftCardPayment]
GO
ALTER TABLE [dbo].[Orders] ADD  CONSTRAINT [DF_Orders_ItemHeldOrDefered]  DEFAULT ((0)) FOR [ItemsHeldOrDefered]
GO
ALTER TABLE [dbo].[Orders] ADD  CONSTRAINT [DF_Orders_CoversChildren]  DEFAULT ((0)) FOR [CoversChildren]
GO
ALTER TABLE [dbo].[Orders] ADD  CONSTRAINT [DF_Orders_isPending]  DEFAULT ((0)) FOR [IsPending]
GO
ALTER TABLE [dbo].[OrdersOffers] ADD  DEFAULT ((1)) FOR [RecordAdded]
GO
ALTER TABLE [dbo].[OrdersOffers] ADD  DEFAULT ((0)) FOR [RecordEdited]
GO
ALTER TABLE [dbo].[OrderTypeDisabledSalesButtons] ADD  DEFAULT ((1)) FOR [RecordAdded]
GO
ALTER TABLE [dbo].[OrderTypeDisabledSalesButtons] ADD  DEFAULT ((0)) FOR [RecordEdited]
GO
ALTER TABLE [dbo].[OrderTypes] ADD  CONSTRAINT [DF_OrderTypes_OrderTypeLID]  DEFAULT ((1)) FOR [OrderTypeLID]
GO
ALTER TABLE [dbo].[OrderTypes] ADD  CONSTRAINT [DF_OrderTypes_IsOnTheFly]  DEFAULT ((0)) FOR [IsOnTheFly]
GO
ALTER TABLE [dbo].[OrderTypes] ADD  CONSTRAINT [DF_OrderTypes_AllowAssignTo]  DEFAULT ((1)) FOR [AllowAssignTo]
GO
ALTER TABLE [dbo].[OrderTypes] ADD  CONSTRAINT [DF_OrderTypes_ApplyVAT]  DEFAULT ((1)) FOR [ApplyVatToAll]
GO
ALTER TABLE [dbo].[OrderTypes] ADD  CONSTRAINT [DF_OrderTypes_ApplyService]  DEFAULT ((0)) FOR [ApplyService]
GO
ALTER TABLE [dbo].[OrderTypes] ADD  CONSTRAINT [DF_OrderTypes_ApplyCover]  DEFAULT ((0)) FOR [ApplyCover]
GO
ALTER TABLE [dbo].[OrderTypes] ADD  CONSTRAINT [DF_OrderTypes_IsDeafult]  DEFAULT ((0)) FOR [IsDefault]
GO
ALTER TABLE [dbo].[OrderTypes] ADD  CONSTRAINT [DF_OrderTypes_IsAcrchived]  DEFAULT ((0)) FOR [IsAcrchived]
GO
ALTER TABLE [dbo].[OrderTypes] ADD  CONSTRAINT [DF_OrderTypes_IsTakeaway]  DEFAULT ((0)) FOR [IsTakeaway]
GO
ALTER TABLE [dbo].[OrderTypes] ADD  CONSTRAINT [DF_OrderTypes_IsDelivery]  DEFAULT ((0)) FOR [IsDelivery]
GO
ALTER TABLE [dbo].[OrderTypes] ADD  CONSTRAINT [DF_OrderTypes_PricePoint]  DEFAULT ((1)) FOR [PricePointAID]
GO
ALTER TABLE [dbo].[OrderTypes] ADD  CONSTRAINT [DF_OrderTypes_PricePointLID]  DEFAULT ((1)) FOR [PricePointLID]
GO
ALTER TABLE [dbo].[OrderTypes] ADD  CONSTRAINT [DF_OrderTypes_AccountPayment]  DEFAULT ((0)) FOR [AccountPayment]
GO
ALTER TABLE [dbo].[OrderTypes] ADD  CONSTRAINT [DF_OrderTypes_IsAccommodation]  DEFAULT ((0)) FOR [IsAccommodation]
GO
ALTER TABLE [dbo].[OrderTypes] ADD  DEFAULT ((1)) FOR [RecordAdded]
GO
ALTER TABLE [dbo].[OrderTypes] ADD  DEFAULT ((0)) FOR [RecordEdited]
GO
ALTER TABLE [dbo].[OrderTypes] ADD  CONSTRAINT [DF_OrderTypes_IsTable]  DEFAULT ((0)) FOR [IsTable]
GO
ALTER TABLE [dbo].[OrderTypes] ADD  CONSTRAINT [DF_OrderTypes_PhoneAssign]  DEFAULT ((1)) FOR [PhoneAssign]
GO
ALTER TABLE [dbo].[OrderTypes] ADD  CONSTRAINT [DF_OrderTypes_ScreenOrderColour]  DEFAULT ((-16777216)) FOR [KitchenScreenColour]
GO
ALTER TABLE [dbo].[OrderTypes] ADD  CONSTRAINT [DF_OrderTypes_DepositPayment]  DEFAULT ((0)) FOR [DepositPayment]
GO
ALTER TABLE [dbo].[OrderTypes] ADD  CONSTRAINT [DF_OrderTypes_ConvertOnTheFlyToThis]  DEFAULT ((0)) FOR [ConvertOnTheFlyToThis]
GO
ALTER TABLE [dbo].[OrderTypes] ADD  CONSTRAINT [DF_OrderTypes_UseTakeawayIngredients]  DEFAULT ((0)) FOR [UseTakeawayIngredients]
GO
ALTER TABLE [dbo].[OrderTypes] ADD  CONSTRAINT [DF_OrderTypes_ScreenPriority]  DEFAULT ((0)) FOR [ScreenPriority]
GO
ALTER TABLE [dbo].[ProductOptions] ADD  DEFAULT ((1)) FOR [RecordAdded]
GO
ALTER TABLE [dbo].[ProductOptions] ADD  DEFAULT ((0)) FOR [RecordEdited]
GO
ALTER TABLE [dbo].[ProductPrices] ADD  DEFAULT ((1)) FOR [RecordAdded]
GO
ALTER TABLE [dbo].[ProductPrices] ADD  DEFAULT ((0)) FOR [RecordEdited]
GO
ALTER TABLE [dbo].[Products] ADD  CONSTRAINT [DF_Products_ProductLID]  DEFAULT ((1)) FOR [ProductLID]
GO
ALTER TABLE [dbo].[Products] ADD  CONSTRAINT [DF_Products_CategoryLID]  DEFAULT ((1)) FOR [SubcategoryLID]
GO
ALTER TABLE [dbo].[Products] ADD  CONSTRAINT [DF_Products_ProductType]  DEFAULT ((0)) FOR [ProductType]
GO
ALTER TABLE [dbo].[Products] ADD  CONSTRAINT [DF_Products_VATCode]  DEFAULT (N'S') FOR [VATCode]
GO
ALTER TABLE [dbo].[Products] ADD  CONSTRAINT [DF_Products_VAT]  DEFAULT ((1)) FOR [VAT]
GO
ALTER TABLE [dbo].[Products] ADD  CONSTRAINT [DF_Products_ScaleItem]  DEFAULT ((0)) FOR [ScaleItem]
GO
ALTER TABLE [dbo].[Products] ADD  CONSTRAINT [DF_Products_IsArchived]  DEFAULT ((0)) FOR [IsArchived]
GO
ALTER TABLE [dbo].[Products] ADD  CONSTRAINT [DF_Products_IsModifyer]  DEFAULT ((0)) FOR [IsModifyer]
GO
ALTER TABLE [dbo].[Products] ADD  CONSTRAINT [DF_Products_Duration]  DEFAULT ((60)) FOR [Duration]
GO
ALTER TABLE [dbo].[Products] ADD  CONSTRAINT [DF_Products_PreTime]  DEFAULT ((0)) FOR [PreTime]
GO
ALTER TABLE [dbo].[Products] ADD  CONSTRAINT [DF_Products_CleanupTime]  DEFAULT ((0)) FOR [CleanupTime]
GO
ALTER TABLE [dbo].[Products] ADD  CONSTRAINT [DF_Products_Manufactured]  DEFAULT ((0)) FOR [Manufactured]
GO
ALTER TABLE [dbo].[Products] ADD  CONSTRAINT [DF_Products_VirtualProduct]  DEFAULT ((0)) FOR [VirtualProduct]
GO
ALTER TABLE [dbo].[Products] ADD  CONSTRAINT [DF_Products_IsLarge]  DEFAULT ((0)) FOR [IsLarge]
GO
ALTER TABLE [dbo].[Products] ADD  DEFAULT ((1)) FOR [RecordAdded]
GO
ALTER TABLE [dbo].[Products] ADD  DEFAULT ((0)) FOR [RecordEdited]
GO
ALTER TABLE [dbo].[Products] ADD  CONSTRAINT [DF_Products_ShowOnWebsite]  DEFAULT ((0)) FOR [ShowOnWebsite]
GO
ALTER TABLE [dbo].[Products] ADD  CONSTRAINT [DF_Products_FeaturedProduct]  DEFAULT ((0)) FOR [FeaturedProduct]
GO
ALTER TABLE [dbo].[Products] ADD  CONSTRAINT [DF_Products_OurPicksProduct]  DEFAULT ((0)) FOR [OurPicksProduct]
GO
ALTER TABLE [dbo].[Products] ADD  CONSTRAINT [DF_Products_IsVegetarian]  DEFAULT ((0)) FOR [IsVegetarian]
GO
ALTER TABLE [dbo].[Products] ADD  CONSTRAINT [DF_Products_CannotBuyOnWebsite]  DEFAULT ((0)) FOR [CannotBuyOnWebsite]
GO
ALTER TABLE [dbo].[Products] ADD  CONSTRAINT [DF_Products_BestSeller]  DEFAULT ((0)) FOR [BestSeller]
GO
ALTER TABLE [dbo].[Products] ADD  CONSTRAINT [DF_Products_NoServiceCharge]  DEFAULT ((0)) FOR [NoServiceChargeItem]
GO
ALTER TABLE [dbo].[Products] ADD  CONSTRAINT [DF_Products_AutoAddToOrders]  DEFAULT ((0)) FOR [AutoAddToOrders]
GO
ALTER TABLE [dbo].[Products] ADD  CONSTRAINT [DF_Products_AppendAltReceiptText]  DEFAULT ((0)) FOR [AppendAltReceiptText]
GO
ALTER TABLE [dbo].[Products] ADD  CONSTRAINT [DF_Products_AppendAltKitchenText]  DEFAULT ((0)) FOR [AppendAltKitchenText]
GO
ALTER TABLE [dbo].[Products] ADD  CONSTRAINT [DF_Products_HideOnScreens]  DEFAULT ((0)) FOR [HideOnScreens]
GO
ALTER TABLE [dbo].[Stock] ADD  CONSTRAINT [DF_Stock_StockLID]  DEFAULT ((1)) FOR [StockLID]
GO
ALTER TABLE [dbo].[Stock] ADD  CONSTRAINT [DF_Stock_StockItemLID]  DEFAULT ((1)) FOR [StockItemLID]
GO
ALTER TABLE [dbo].[Stock] ADD  DEFAULT ((1)) FOR [RecordAdded]
GO
ALTER TABLE [dbo].[Stock] ADD  DEFAULT ((0)) FOR [RecordEdited]
GO
ALTER TABLE [dbo].[StockCategories] ADD  DEFAULT ((1)) FOR [RecordAdded]
GO
ALTER TABLE [dbo].[StockCategories] ADD  DEFAULT ((0)) FOR [RecordEdited]
GO
ALTER TABLE [dbo].[StockItemBatches] ADD  CONSTRAINT [DF_StockItemBatches_Quantity]  DEFAULT ((0)) FOR [Quantity]
GO
ALTER TABLE [dbo].[StockItemBatches] ADD  CONSTRAINT [DF_StockItemBatches_QuantityLeft]  DEFAULT ((0)) FOR [QuantityLeft]
GO
ALTER TABLE [dbo].[StockItemIngredients] ADD  DEFAULT ((1)) FOR [RecordAdded]
GO
ALTER TABLE [dbo].[StockItemIngredients] ADD  DEFAULT ((0)) FOR [RecordEdited]
GO
ALTER TABLE [dbo].[StockItemIngredients] ADD  CONSTRAINT [DF_StockItemIngredients_TakeawayOnly]  DEFAULT ((0)) FOR [TakeawayOnly]
GO
ALTER TABLE [dbo].[StockItems] ADD  CONSTRAINT [DF_StockItems_StockItemLID]  DEFAULT ((1)) FOR [StockItemLID]
GO
ALTER TABLE [dbo].[StockItems] ADD  CONSTRAINT [DF_StockItems_ProductLID]  DEFAULT ((1)) FOR [ProductLID]
GO
ALTER TABLE [dbo].[StockItems] ADD  CONSTRAINT [DF_StockItems_VATCode]  DEFAULT (N'S') FOR [VATCode]
GO
ALTER TABLE [dbo].[StockItems] ADD  CONSTRAINT [DF_StockItems_VAT]  DEFAULT ((0)) FOR [VAT]
GO
ALTER TABLE [dbo].[StockItems] ADD  CONSTRAINT [DF_StockItems_IsArchived]  DEFAULT ((0)) FOR [IsArchived]
GO
ALTER TABLE [dbo].[StockItems] ADD  CONSTRAINT [DF_StockItems_CanBeIngredient]  DEFAULT ((0)) FOR [CanBeIngredient]
GO
ALTER TABLE [dbo].[StockItems] ADD  CONSTRAINT [DF_StockItems_StockTakeHigherTolerance]  DEFAULT ((0)) FOR [StockTakeHigherTolerance]
GO
ALTER TABLE [dbo].[StockItems] ADD  CONSTRAINT [DF_StockItems_StockTakeLowerTolerance]  DEFAULT ((0)) FOR [StockTakeLowerTolerance]
GO
ALTER TABLE [dbo].[StockItems] ADD  CONSTRAINT [DF_StockItems_StockTakeRoundDown]  DEFAULT ((0)) FOR [StockTakeRoundDown]
GO
ALTER TABLE [dbo].[StockItems] ADD  CONSTRAINT [DF_StockItems_BaseUnitAID]  DEFAULT ((1)) FOR [BaseUnitAID]
GO
ALTER TABLE [dbo].[StockItems] ADD  CONSTRAINT [DF_StockItems_BaseUnitLID]  DEFAULT ((1)) FOR [BaseUnitLID]
GO
ALTER TABLE [dbo].[StockItems] ADD  CONSTRAINT [DF_StockItems_SalesUnitAID]  DEFAULT ((1)) FOR [SalesUnitAID]
GO
ALTER TABLE [dbo].[StockItems] ADD  CONSTRAINT [DF_StockItems_SalesUnitLID]  DEFAULT ((1)) FOR [SalesUnitLID]
GO
ALTER TABLE [dbo].[StockItems] ADD  CONSTRAINT [DF_StockItems_IsAggregate]  DEFAULT ((0)) FOR [IsCombination]
GO
ALTER TABLE [dbo].[StockItems] ADD  CONSTRAINT [DF_StockItems_CombineOnSale]  DEFAULT ((1)) FOR [CombineOnSale]
GO
ALTER TABLE [dbo].[StockItems] ADD  DEFAULT ((1)) FOR [RecordAdded]
GO
ALTER TABLE [dbo].[StockItems] ADD  DEFAULT ((0)) FOR [RecordEdited]
GO
ALTER TABLE [dbo].[StockItems] ADD  CONSTRAINT [DF_StockItems_OnlyManufacturedInProduction]  DEFAULT ((0)) FOR [OnlyManufacturedInProduction]
GO
ALTER TABLE [dbo].[StockItems] ADD  CONSTRAINT [DF_StockItems_StockItemType]  DEFAULT ((0)) FOR [StockItemType]
GO
ALTER TABLE [dbo].[StockItems] ADD  CONSTRAINT [DF_StockItems_GenerateBatchNumbers]  DEFAULT ((0)) FOR [GenerateBatchNumbers]
GO
ALTER TABLE [dbo].[StockItems] ADD  CONSTRAINT [DF_StockItems_CostChanged]  DEFAULT ((0)) FOR [CostChanged]
GO
ALTER TABLE [dbo].[StockItems] ADD  CONSTRAINT [DF_StockItems_DepletionType]  DEFAULT ((0)) FOR [DepletionType]
GO
ALTER TABLE [dbo].[StockItems] ADD  CONSTRAINT [DF_StockItems_HideInStockTake]  DEFAULT ((0)) FOR [HideInStockTake]
GO
ALTER TABLE [dbo].[StockItemSize] ADD  DEFAULT ((1)) FOR [RecordAdded]
GO
ALTER TABLE [dbo].[StockItemSize] ADD  DEFAULT ((0)) FOR [RecordEdited]
GO
ALTER TABLE [dbo].[StockItemSizeIngredients] ADD  DEFAULT ((1)) FOR [RecordAdded]
GO
ALTER TABLE [dbo].[StockItemSizeIngredients] ADD  DEFAULT ((0)) FOR [RecordEdited]
GO
ALTER TABLE [dbo].[StockItemSizeIngredients] ADD  CONSTRAINT [DF_StockItemSizeIngredients_TakeawayOnly]  DEFAULT ((0)) FOR [TakeawayOnly]
GO
ALTER TABLE [dbo].[StockItemSuppliers] ADD  CONSTRAINT [DF_StockItemSuppliers_StockItemAID]  DEFAULT ((1)) FOR [StockItemLID]
GO
ALTER TABLE [dbo].[StockItemSuppliers] ADD  CONSTRAINT [DF_StockItemSuppliers_SupplierLID]  DEFAULT ((1)) FOR [SupplierLID]
GO
ALTER TABLE [dbo].[StockItemSuppliers] ADD  CONSTRAINT [DF_StockItemSuppliers_SupplyUnitAID]  DEFAULT ((1)) FOR [SupplyUnitAID]
GO
ALTER TABLE [dbo].[StockItemSuppliers] ADD  CONSTRAINT [DF_StockItemSuppliers_SupplyUnitLID]  DEFAULT ((1)) FOR [SupplyUnitLID]
GO
ALTER TABLE [dbo].[StockItemSuppliers] ADD  CONSTRAINT [DF_StockItemSuppliers_VATCode]  DEFAULT (N'T1') FOR [VATCode]
GO
ALTER TABLE [dbo].[StockItemSuppliers] ADD  DEFAULT ((1)) FOR [RecordAdded]
GO
ALTER TABLE [dbo].[StockItemSuppliers] ADD  DEFAULT ((0)) FOR [RecordEdited]
GO
ALTER TABLE [dbo].[StockOrderItems] ADD  CONSTRAINT [DF_StockOrderItems_StockOrderItemLID]  DEFAULT ((1)) FOR [StockOrderItemLID]
GO
ALTER TABLE [dbo].[StockOrderItems] ADD  CONSTRAINT [DF_StockOrderItems_StockOrderLID]  DEFAULT ((1)) FOR [StockOrderLID]
GO
ALTER TABLE [dbo].[StockOrderItems] ADD  CONSTRAINT [DF_StockOrderItems_StockItemLID]  DEFAULT ((1)) FOR [StockItemLID]
GO
ALTER TABLE [dbo].[StockOrderItems] ADD  CONSTRAINT [DF_StockOrderItems_VATCode]  DEFAULT (N'S') FOR [VATCode]
GO
ALTER TABLE [dbo].[StockOrderItems] ADD  CONSTRAINT [DF_StockOrderItems_PartReceived]  DEFAULT ((0)) FOR [PartReceived]
GO
ALTER TABLE [dbo].[StockOrderItems] ADD  CONSTRAINT [DF_StockOrderItems_UnitAID]  DEFAULT ((1)) FOR [UnitAID]
GO
ALTER TABLE [dbo].[StockOrderItems] ADD  CONSTRAINT [DF_StockOrderItems_UnitLID]  DEFAULT ((1)) FOR [UnitLID]
GO
ALTER TABLE [dbo].[StockOrderItems] ADD  DEFAULT ((1)) FOR [RecordAdded]
GO
ALTER TABLE [dbo].[StockOrderItems] ADD  DEFAULT ((0)) FOR [RecordEdited]
GO
ALTER TABLE [dbo].[StockOrderNumbers] ADD  DEFAULT ((1)) FOR [RecordAdded]
GO
ALTER TABLE [dbo].[StockOrderNumbers] ADD  DEFAULT ((0)) FOR [RecordEdited]
GO
ALTER TABLE [dbo].[StockOrders] ADD  CONSTRAINT [DF_StockOrders_StockOrderLID]  DEFAULT ((1)) FOR [StockOrderLID]
GO
ALTER TABLE [dbo].[StockOrders] ADD  CONSTRAINT [DF_StockOrders_SupplilerLID]  DEFAULT ((1)) FOR [SupplierLID]
GO
ALTER TABLE [dbo].[StockOrders] ADD  CONSTRAINT [DF_StockOrders_StockOrderState]  DEFAULT ((0)) FOR [StockOrderState]
GO
ALTER TABLE [dbo].[StockOrders] ADD  CONSTRAINT [DF_StockOrders_CreationUserLID]  DEFAULT ((1)) FOR [CreationUserLID]
GO
ALTER TABLE [dbo].[StockOrders] ADD  CONSTRAINT [DF_StockOrders_LocalToBaseCurrencyRate]  DEFAULT ((1)) FOR [OrderToBaseCurrencyRate]
GO
ALTER TABLE [dbo].[StockOrders] ADD  CONSTRAINT [DF_StockOrders_Recurrence]  DEFAULT ((0)) FOR [Recurrence]
GO
ALTER TABLE [dbo].[StockOrders] ADD  CONSTRAINT [DF_StockOrders_SendToSage]  DEFAULT ((0)) FOR [SendToSage]
GO
ALTER TABLE [dbo].[StockOrders] ADD  CONSTRAINT [DF_StockOrders_InSage]  DEFAULT ((0)) FOR [InSage]
GO
ALTER TABLE [dbo].[StockOrders] ADD  DEFAULT ((1)) FOR [RecordAdded]
GO
ALTER TABLE [dbo].[StockOrders] ADD  DEFAULT ((0)) FOR [RecordEdited]
GO
ALTER TABLE [dbo].[StockOrders] ADD  CONSTRAINT [DF_StockOrders_Emailed]  DEFAULT ((0)) FOR [Emailed]
GO
ALTER TABLE [dbo].[StockOrders] ADD  CONSTRAINT [DF_StockOrders_IsSubmitted]  DEFAULT ((1)) FOR [IsSubmitted]
GO
ALTER TABLE [dbo].[StockOrders] ADD  CONSTRAINT [DF_StockOrders_OnlinceOrderState]  DEFAULT ((0)) FOR [OnlineOrderState]
GO
ALTER TABLE [dbo].[StockOrders] ADD  CONSTRAINT [DF_StockOrders_SendToBestway]  DEFAULT ((0)) FOR [SendToBestway]
GO
ALTER TABLE [dbo].[StockOrders] ADD  CONSTRAINT [DF_StockOrders_SentToBestway]  DEFAULT ((0)) FOR [SentToBestway]
GO
ALTER TABLE [dbo].[StockOrders] ADD  CONSTRAINT [DF_StockOrders_SendChefDirectOrder]  DEFAULT ((0)) FOR [SendChefDirectOrder]
GO
ALTER TABLE [dbo].[StockOrders] ADD  CONSTRAINT [DF_StockOrders_SentChefDirectOrder]  DEFAULT ((0)) FOR [SentChefDirectOrder]
GO
ALTER TABLE [dbo].[StockOrders] ADD  CONSTRAINT [DF_StockOrders_DeliveryCharge]  DEFAULT ((0)) FOR [DeliveryCharge]
GO
ALTER TABLE [dbo].[StockTake] ADD  CONSTRAINT [DF_StockTake_AppliedToStock]  DEFAULT ((1)) FOR [AppliedToStock]
GO
ALTER TABLE [dbo].[StockTake] ADD  CONSTRAINT [DF_StockTake_HeldUntilLocal]  DEFAULT ((0)) FOR [HeldUntilLocal]
GO
ALTER TABLE [dbo].[StockTake] ADD  CONSTRAINT [DF_StockTake_IsSnapshot]  DEFAULT ((0)) FOR [IsSnapshot]
GO
ALTER TABLE [dbo].[StockTake] ADD  DEFAULT ((1)) FOR [RecordAdded]
GO
ALTER TABLE [dbo].[StockTake] ADD  DEFAULT ((0)) FOR [RecordEdited]
GO
ALTER TABLE [dbo].[StockTakeItem] ADD  CONSTRAINT [DF_StockTakeItem_Applied]  DEFAULT ((1)) FOR [Applied]
GO
ALTER TABLE [dbo].[StockTakeItem] ADD  CONSTRAINT [DF_StockTakeItem_UnisCost]  DEFAULT ((0)) FOR [StockUnitCost]
GO
ALTER TABLE [dbo].[StockTakeItem] ADD  DEFAULT ((1)) FOR [RecordAdded]
GO
ALTER TABLE [dbo].[StockTakeItem] ADD  DEFAULT ((0)) FOR [RecordEdited]
GO
ALTER TABLE [dbo].[StockTakeItem] ADD  CONSTRAINT [DF_StockTakeItem_OutOfRange]  DEFAULT ((0)) FOR [OutOfRange]
GO
ALTER TABLE [dbo].[StockTransfers] ADD  CONSTRAINT [DF_StockTransfers_Received]  DEFAULT ((0)) FOR [Received]
GO
ALTER TABLE [dbo].[StockTransfers] ADD  CONSTRAINT [DF_StockTransfers_Cancelled]  DEFAULT ((0)) FOR [Cancelled]
GO
ALTER TABLE [dbo].[StockTransfers] ADD  CONSTRAINT [DF_StockTransfers_ReductionHeldUntilLocal]  DEFAULT ((0)) FOR [CheckoutHeldUntilLocal]
GO
ALTER TABLE [dbo].[StockTransfers] ADD  DEFAULT ((1)) FOR [RecordAdded]
GO
ALTER TABLE [dbo].[StockTransfers] ADD  DEFAULT ((0)) FOR [RecordEdited]
GO
ALTER TABLE [dbo].[WebOrderDetails] ADD  CONSTRAINT [DF_WebOrderDetails_Viewed]  DEFAULT ((0)) FOR [Viewed]
GO
ALTER TABLE [dbo].[WebOrderDetails] ADD  DEFAULT ((1)) FOR [RecordAdded]
GO
ALTER TABLE [dbo].[WebOrderDetails] ADD  DEFAULT ((0)) FOR [RecordEdited]
GO
ALTER TABLE [dbo].[WebOrderDetails] ADD  CONSTRAINT [DF_WebOrderDetails_IsAppOrder]  DEFAULT ((0)) FOR [IsAppOrder]
GO
ALTER TABLE [dbo].[WebOrderDetails] ADD  CONSTRAINT [DF_WebOrderDetails_NumAdultAllYouCanEat]  DEFAULT ((0)) FOR [NumAdultAllYouCanEat]
GO
ALTER TABLE [dbo].[WebOrderDetails] ADD  CONSTRAINT [DF_WebOrderDetails_NumChildAllYouCanEat]  DEFAULT ((0)) FOR [NumChildAllYouCanEat]
GO
ALTER TABLE [dbo].[WebOrderDetails] ADD  CONSTRAINT [DF_WebOrderDetails_NumALaCarte]  DEFAULT ((0)) FOR [NumALaCarte]
GO
ALTER TABLE [dbo].[WebOrderDetails] ADD  CONSTRAINT [DF_WebOrderDetails_BillRequested]  DEFAULT ((0)) FOR [BillRequested]
GO
ALTER TABLE [dbo].[WebOrderDetails] ADD  CONSTRAINT [DF_WebOrderDetails_WaiterCalled]  DEFAULT ((0)) FOR [WaiterCalled]
GO
ALTER TABLE [dbo].[WebOrderDetails] ADD  CONSTRAINT [DF_WebOrderDetails_ItemsPending]  DEFAULT ((0)) FOR [ItemsPending]
GO
ALTER TABLE [dbo].[OrderBookingInfo]  WITH CHECK ADD  CONSTRAINT [FK_OrderBookingInfo_Orders] FOREIGN KEY([OrderAID], [OrderLID])
REFERENCES [dbo].[Orders] ([OrderAID], [OrderLID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[OrderBookingInfo] CHECK CONSTRAINT [FK_OrderBookingInfo_Orders]
GO
ALTER TABLE [dbo].[OrderItems]  WITH NOCHECK ADD  CONSTRAINT [FK_OrderItems_OrderItems1] FOREIGN KEY([ModifyOrderItemAID], [ModifyOrderItemLID])
REFERENCES [dbo].[OrderItems] ([OrderItemAID], [OrderItemLID])
GO
ALTER TABLE [dbo].[OrderItems] CHECK CONSTRAINT [FK_OrderItems_OrderItems1]
GO
ALTER TABLE [dbo].[OrderItems]  WITH NOCHECK ADD  CONSTRAINT [FK_OrderItems_Orders1] FOREIGN KEY([OrderAID], [OrderLID])
REFERENCES [dbo].[Orders] ([OrderAID], [OrderLID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[OrderItems] CHECK CONSTRAINT [FK_OrderItems_Orders1]
GO
ALTER TABLE [dbo].[OrderItems]  WITH NOCHECK ADD  CONSTRAINT [FK_OrderItems_Products1] FOREIGN KEY([ProductAID], [ProductLID])
REFERENCES [dbo].[Products] ([ProductAID], [ProductLID])
ON UPDATE SET NULL
ON DELETE SET NULL
GO
ALTER TABLE [dbo].[OrderItems] CHECK CONSTRAINT [FK_OrderItems_Products1]
GO
ALTER TABLE [dbo].[OrderItems]  WITH NOCHECK ADD  CONSTRAINT [FK_OrderItems_Reasons] FOREIGN KEY([RefundReasonAID], [RefundReasonLID])
REFERENCES [dbo].[Reasons] ([ReasonAID], [ReasonLID])
GO
ALTER TABLE [dbo].[OrderItems] CHECK CONSTRAINT [FK_OrderItems_Reasons]
GO
ALTER TABLE [dbo].[OrderItemsAltTexts]  WITH CHECK ADD  CONSTRAINT [FK_OrderItemsAltTexts_OrderItems] FOREIGN KEY([OrderItemAID], [OrderItemLID])
REFERENCES [dbo].[OrderItems] ([OrderItemAID], [OrderItemLID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[OrderItemsAltTexts] CHECK CONSTRAINT [FK_OrderItemsAltTexts_OrderItems]
GO
ALTER TABLE [dbo].[OrderItemSpecs]  WITH NOCHECK ADD  CONSTRAINT [FK_OrderItemSpecs_OrderItems] FOREIGN KEY([OrderItemAID], [OrderItemLID])
REFERENCES [dbo].[OrderItems] ([OrderItemAID], [OrderItemLID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[OrderItemSpecs] CHECK CONSTRAINT [FK_OrderItemSpecs_OrderItems]
GO
ALTER TABLE [dbo].[OrderItemSpecs]  WITH NOCHECK ADD  CONSTRAINT [FK_OrderItemSpecs_Sizes] FOREIGN KEY([SizeAID], [SizeLID])
REFERENCES [dbo].[Sizes] ([SizeAID], [SizeLID])
GO
ALTER TABLE [dbo].[OrderItemSpecs] CHECK CONSTRAINT [FK_OrderItemSpecs_Sizes]
GO
ALTER TABLE [dbo].[OrderlordItems]  WITH CHECK ADD  CONSTRAINT [FK_OrderlordItems_Orders] FOREIGN KEY([OrderAID], [OrderLID])
REFERENCES [dbo].[Orders] ([OrderAID], [OrderLID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[OrderlordItems] CHECK CONSTRAINT [FK_OrderlordItems_Orders]
GO
ALTER TABLE [dbo].[Orders]  WITH NOCHECK ADD  CONSTRAINT [FK_Orders_Customers] FOREIGN KEY([CustomerAID], [CustomerLID])
REFERENCES [dbo].[Customers] ([CustomerAID], [CustomerLID])
ON UPDATE SET NULL
ON DELETE SET NULL
GO
ALTER TABLE [dbo].[Orders] CHECK CONSTRAINT [FK_Orders_Customers]
GO
ALTER TABLE [dbo].[Orders]  WITH NOCHECK ADD  CONSTRAINT [FK_Orders_EodCategory] FOREIGN KEY([EodCategoryAID], [EodCategoryLID])
REFERENCES [dbo].[EodCategory] ([EodCategoryAID], [EodCategoryLID])
GO
ALTER TABLE [dbo].[Orders] CHECK CONSTRAINT [FK_Orders_EodCategory]
GO
ALTER TABLE [dbo].[Orders]  WITH NOCHECK ADD  CONSTRAINT [FK_Orders_Locations] FOREIGN KEY([LocationID])
REFERENCES [dbo].[Locations] ([LocationID])
GO
ALTER TABLE [dbo].[Orders] CHECK CONSTRAINT [FK_Orders_Locations]
GO
ALTER TABLE [dbo].[Orders]  WITH NOCHECK ADD  CONSTRAINT [FK_Orders_OrderTypes] FOREIGN KEY([OrderTypeAID], [OrderTypeLID])
REFERENCES [dbo].[OrderTypes] ([OrderTypeAID], [OrderTypeLID])
GO
ALTER TABLE [dbo].[Orders] CHECK CONSTRAINT [FK_Orders_OrderTypes]
GO
ALTER TABLE [dbo].[Orders]  WITH NOCHECK ADD  CONSTRAINT [FK_Orders_Reasons] FOREIGN KEY([DiscountReasonAID], [DiscountReasonLID])
REFERENCES [dbo].[Reasons] ([ReasonAID], [ReasonLID])
ON UPDATE SET NULL
ON DELETE SET NULL
GO
ALTER TABLE [dbo].[Orders] CHECK CONSTRAINT [FK_Orders_Reasons]
GO
ALTER TABLE [dbo].[OrdersOffers]  WITH NOCHECK ADD  CONSTRAINT [FK_OrdersOffers_DiscountRules] FOREIGN KEY([DiscountRuleAID], [DiscountRuleLID])
REFERENCES [dbo].[DiscountRules] ([DiscountRuleAID], [DiscountRuleLID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[OrdersOffers] CHECK CONSTRAINT [FK_OrdersOffers_DiscountRules]
GO
ALTER TABLE [dbo].[OrdersOffers]  WITH NOCHECK ADD  CONSTRAINT [FK_OrdersOffers_Orders] FOREIGN KEY([OrderAID], [OrderLID])
REFERENCES [dbo].[Orders] ([OrderAID], [OrderLID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[OrdersOffers] CHECK CONSTRAINT [FK_OrdersOffers_Orders]
GO
ALTER TABLE [dbo].[OrderTypeDisabledSalesButtons]  WITH CHECK ADD  CONSTRAINT [FK_OrderTypeDisabledSalesButtons_OrderTypes] FOREIGN KEY([OrderTypeAID], [OrderTypeLID])
REFERENCES [dbo].[OrderTypes] ([OrderTypeAID], [OrderTypeLID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[OrderTypeDisabledSalesButtons] CHECK CONSTRAINT [FK_OrderTypeDisabledSalesButtons_OrderTypes]
GO
ALTER TABLE [dbo].[OrderTypeDisabledSalesButtons]  WITH CHECK ADD  CONSTRAINT [FK_OrderTypeDisabledSalesButtons_SalesButtons] FOREIGN KEY([SalesButtonAID], [SalesButtonLID])
REFERENCES [dbo].[SalesButtons] ([SalesButtonAID], [SalesButtonLID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[OrderTypeDisabledSalesButtons] CHECK CONSTRAINT [FK_OrderTypeDisabledSalesButtons_SalesButtons]
GO
ALTER TABLE [dbo].[OrderTypes]  WITH NOCHECK ADD  CONSTRAINT [FK_OrderTypes_VatScheme] FOREIGN KEY([VatSchemeAID], [VatSchemeLID])
REFERENCES [dbo].[VatScheme] ([VatSchemeAID], [VatSchemeLID])
ON UPDATE SET NULL
ON DELETE SET NULL
GO
ALTER TABLE [dbo].[OrderTypes] CHECK CONSTRAINT [FK_OrderTypes_VatScheme]
GO
ALTER TABLE [dbo].[ProductPrices]  WITH NOCHECK ADD  CONSTRAINT [FK_ProductPrices_PricePoints] FOREIGN KEY([PricePointAID], [PricePointLID])
REFERENCES [dbo].[PricePoints] ([PricePointAID], [PricePointLID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[ProductPrices] CHECK CONSTRAINT [FK_ProductPrices_PricePoints]
GO
ALTER TABLE [dbo].[ProductPrices]  WITH NOCHECK ADD  CONSTRAINT [FK_ProductPrices_Products] FOREIGN KEY([ProductAID], [ProductLID])
REFERENCES [dbo].[Products] ([ProductAID], [ProductLID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[ProductPrices] CHECK CONSTRAINT [FK_ProductPrices_Products]
GO
ALTER TABLE [dbo].[Products]  WITH NOCHECK ADD  CONSTRAINT [FK_Products_ProductCategories] FOREIGN KEY([SubcategoryAID], [SubcategoryLID])
REFERENCES [dbo].[Subcategories] ([SubcategoryAID], [SubcategoryLID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Products] CHECK CONSTRAINT [FK_Products_ProductCategories]
GO
ALTER TABLE [dbo].[Products]  WITH NOCHECK ADD  CONSTRAINT [FK_Products_ProductFamily] FOREIGN KEY([ProductFamilyAID], [ProductFamilyLID])
REFERENCES [dbo].[ProductFamily] ([ProductFamilyAID], [ProductFamilyLID])
ON UPDATE SET NULL
ON DELETE SET NULL
GO
ALTER TABLE [dbo].[Products] CHECK CONSTRAINT [FK_Products_ProductFamily]
GO
ALTER TABLE [dbo].[Products]  WITH NOCHECK ADD  CONSTRAINT [FK_Products_ScreenProductGroups] FOREIGN KEY([ScreenProductGroupAID], [ScreenProductGroupLID])
REFERENCES [dbo].[ScreenProductGroups] ([ScreenProductGroupAID], [ScreenProductGroupLID])
ON UPDATE SET NULL
ON DELETE SET NULL
GO
ALTER TABLE [dbo].[Products] CHECK CONSTRAINT [FK_Products_ScreenProductGroups]
GO
ALTER TABLE [dbo].[Products]  WITH NOCHECK ADD  CONSTRAINT [FK_Products_VatCodes] FOREIGN KEY([VATCode])
REFERENCES [dbo].[VatCodes] ([VATCode])
GO
ALTER TABLE [dbo].[Products] CHECK CONSTRAINT [FK_Products_VatCodes]
GO
ALTER TABLE [dbo].[Stock]  WITH CHECK ADD  CONSTRAINT [FK_Stock_StockItems1] FOREIGN KEY([StockItemAID], [StockItemLID])
REFERENCES [dbo].[StockItems] ([StockItemAID], [StockItemLID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Stock] CHECK CONSTRAINT [FK_Stock_StockItems1]
GO
ALTER TABLE [dbo].[StockItemBatches]  WITH CHECK ADD  CONSTRAINT [FK_StockItemBatches_Locations] FOREIGN KEY([GeneratedAtLocationID])
REFERENCES [dbo].[Locations] ([LocationID])
GO
ALTER TABLE [dbo].[StockItemBatches] CHECK CONSTRAINT [FK_StockItemBatches_Locations]
GO
ALTER TABLE [dbo].[StockItemBatches]  WITH CHECK ADD  CONSTRAINT [FK_StockItemBatches_Products] FOREIGN KEY([ProductAID], [ProductLID])
REFERENCES [dbo].[Products] ([ProductAID], [ProductLID])
GO
ALTER TABLE [dbo].[StockItemBatches] CHECK CONSTRAINT [FK_StockItemBatches_Products]
GO
ALTER TABLE [dbo].[StockItemBatches]  WITH CHECK ADD  CONSTRAINT [FK_StockItemBatches_Stations] FOREIGN KEY([StationAID], [StationLID])
REFERENCES [dbo].[Stations] ([StationAID], [StationLID])
GO
ALTER TABLE [dbo].[StockItemBatches] CHECK CONSTRAINT [FK_StockItemBatches_Stations]
GO
ALTER TABLE [dbo].[StockItemBatches]  WITH CHECK ADD  CONSTRAINT [FK_StockItemBatches_StockItems] FOREIGN KEY([StockItemAID], [StockItemLID])
REFERENCES [dbo].[StockItems] ([StockItemAID], [StockItemLID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[StockItemBatches] CHECK CONSTRAINT [FK_StockItemBatches_StockItems]
GO
ALTER TABLE [dbo].[StockItemBatches]  WITH CHECK ADD  CONSTRAINT [FK_StockItemBatches_StockOrderItems] FOREIGN KEY([StockOrderItemAID], [StockOrderItemLID])
REFERENCES [dbo].[StockOrderItems] ([StockOrderItemAID], [StockOrderItemLID])
GO
ALTER TABLE [dbo].[StockItemBatches] CHECK CONSTRAINT [FK_StockItemBatches_StockOrderItems]
GO
ALTER TABLE [dbo].[StockItemBatches]  WITH CHECK ADD  CONSTRAINT [FK_StockItemBatches_Users] FOREIGN KEY([GeneratedByUserAID], [GeneratedByUserLID])
REFERENCES [dbo].[Users] ([UserAID], [UserLID])
GO
ALTER TABLE [dbo].[StockItemBatches] CHECK CONSTRAINT [FK_StockItemBatches_Users]
GO
ALTER TABLE [dbo].[StockItemIngredients]  WITH CHECK ADD  CONSTRAINT [FK_StockItemIngredients_StockItems] FOREIGN KEY([StockItemAID], [StockItemLID])
REFERENCES [dbo].[StockItems] ([StockItemAID], [StockItemLID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[StockItemIngredients] CHECK CONSTRAINT [FK_StockItemIngredients_StockItems]
GO
ALTER TABLE [dbo].[StockItemIngredients]  WITH CHECK ADD  CONSTRAINT [FK_StockItemIngredients_StockItems1] FOREIGN KEY([IngredientStockItemAID], [IngredientStockItemLID])
REFERENCES [dbo].[StockItems] ([StockItemAID], [StockItemLID])
GO
ALTER TABLE [dbo].[StockItemIngredients] CHECK CONSTRAINT [FK_StockItemIngredients_StockItems1]
GO
ALTER TABLE [dbo].[StockItemIngredients]  WITH CHECK ADD  CONSTRAINT [FK_StockItemIngredients_Units] FOREIGN KEY([UnitAID], [UnitLID])
REFERENCES [dbo].[Units] ([UnitAID], [UnitLID])
GO
ALTER TABLE [dbo].[StockItemIngredients] CHECK CONSTRAINT [FK_StockItemIngredients_Units]
GO
ALTER TABLE [dbo].[StockItems]  WITH NOCHECK ADD  CONSTRAINT [FK_StockItems_Products1] FOREIGN KEY([ProductAID], [ProductLID])
REFERENCES [dbo].[Products] ([ProductAID], [ProductLID])
ON UPDATE SET NULL
ON DELETE SET NULL
GO
ALTER TABLE [dbo].[StockItems] CHECK CONSTRAINT [FK_StockItems_Products1]
GO
ALTER TABLE [dbo].[StockItems]  WITH NOCHECK ADD  CONSTRAINT [FK_StockItems_StockCategories] FOREIGN KEY([StockCategoryAID], [StockCategoryLID])
REFERENCES [dbo].[StockCategories] ([StockCategroyAID], [StockCategoryLID])
GO
ALTER TABLE [dbo].[StockItems] CHECK CONSTRAINT [FK_StockItems_StockCategories]
GO
ALTER TABLE [dbo].[StockItems]  WITH NOCHECK ADD  CONSTRAINT [FK_StockItems_Units] FOREIGN KEY([BaseUnitAID], [BaseUnitLID])
REFERENCES [dbo].[Units] ([UnitAID], [UnitLID])
GO
ALTER TABLE [dbo].[StockItems] CHECK CONSTRAINT [FK_StockItems_Units]
GO
ALTER TABLE [dbo].[StockItems]  WITH NOCHECK ADD  CONSTRAINT [FK_StockItems_Units1] FOREIGN KEY([SalesUnitAID], [SalesUnitLID])
REFERENCES [dbo].[Units] ([UnitAID], [UnitLID])
GO
ALTER TABLE [dbo].[StockItems] CHECK CONSTRAINT [FK_StockItems_Units1]
GO
ALTER TABLE [dbo].[StockItems]  WITH NOCHECK ADD  CONSTRAINT [FK_StockItems_VatCodes] FOREIGN KEY([VATCode])
REFERENCES [dbo].[VatCodes] ([VATCode])
GO
ALTER TABLE [dbo].[StockItems] CHECK CONSTRAINT [FK_StockItems_VatCodes]
GO
ALTER TABLE [dbo].[StockItemSize]  WITH NOCHECK ADD  CONSTRAINT [FK_StockItemSize_Sizes] FOREIGN KEY([SizeAID], [SizeLID])
REFERENCES [dbo].[Sizes] ([SizeAID], [SizeLID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[StockItemSize] CHECK CONSTRAINT [FK_StockItemSize_Sizes]
GO
ALTER TABLE [dbo].[StockItemSize]  WITH NOCHECK ADD  CONSTRAINT [FK_StockItemSize_StockItems] FOREIGN KEY([StockItemAID], [StockItemLID])
REFERENCES [dbo].[StockItems] ([StockItemAID], [StockItemLID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[StockItemSize] CHECK CONSTRAINT [FK_StockItemSize_StockItems]
GO
ALTER TABLE [dbo].[StockItemSizeIngredients]  WITH CHECK ADD  CONSTRAINT [FK_StockItemSizeIngredients_StockItems] FOREIGN KEY([StockItemAID], [StockItemLID])
REFERENCES [dbo].[StockItems] ([StockItemAID], [StockItemLID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[StockItemSizeIngredients] CHECK CONSTRAINT [FK_StockItemSizeIngredients_StockItems]
GO
ALTER TABLE [dbo].[StockItemSizeIngredients]  WITH CHECK ADD  CONSTRAINT [FK_StockItemSizeIngredients_StockItems1] FOREIGN KEY([IngredientStockItemAID], [IngredientStockItemLID])
REFERENCES [dbo].[StockItems] ([StockItemAID], [StockItemLID])
GO
ALTER TABLE [dbo].[StockItemSizeIngredients] CHECK CONSTRAINT [FK_StockItemSizeIngredients_StockItems1]
GO
ALTER TABLE [dbo].[StockItemSizeIngredients]  WITH CHECK ADD  CONSTRAINT [FK_StockItemSizeIngredients_Units] FOREIGN KEY([UnitAID], [UnitLID])
REFERENCES [dbo].[Units] ([UnitAID], [UnitLID])
GO
ALTER TABLE [dbo].[StockItemSizeIngredients] CHECK CONSTRAINT [FK_StockItemSizeIngredients_Units]
GO
ALTER TABLE [dbo].[StockItemSuppliers]  WITH NOCHECK ADD  CONSTRAINT [FK_StockItemSuppliers_StockItems1] FOREIGN KEY([StockItemAID], [StockItemLID])
REFERENCES [dbo].[StockItems] ([StockItemAID], [StockItemLID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[StockItemSuppliers] CHECK CONSTRAINT [FK_StockItemSuppliers_StockItems1]
GO
ALTER TABLE [dbo].[StockItemSuppliers]  WITH NOCHECK ADD  CONSTRAINT [FK_StockItemSuppliers_Suppliers1] FOREIGN KEY([SupplierAID], [SupplierLID])
REFERENCES [dbo].[Suppliers] ([SupplierAID], [SupplierLID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[StockItemSuppliers] CHECK CONSTRAINT [FK_StockItemSuppliers_Suppliers1]
GO
ALTER TABLE [dbo].[StockItemSuppliers]  WITH NOCHECK ADD  CONSTRAINT [FK_StockItemSuppliers_Units] FOREIGN KEY([SupplyUnitAID], [SupplyUnitLID])
REFERENCES [dbo].[Units] ([UnitAID], [UnitLID])
GO
ALTER TABLE [dbo].[StockItemSuppliers] CHECK CONSTRAINT [FK_StockItemSuppliers_Units]
GO
ALTER TABLE [dbo].[StockItemSuppliers]  WITH NOCHECK ADD  CONSTRAINT [FK_StockItemSuppliers_VatCodes] FOREIGN KEY([VATCode])
REFERENCES [dbo].[VatCodes] ([VATCode])
GO
ALTER TABLE [dbo].[StockItemSuppliers] CHECK CONSTRAINT [FK_StockItemSuppliers_VatCodes]
GO
ALTER TABLE [dbo].[StockOrderItems]  WITH CHECK ADD  CONSTRAINT [FK_StockOrderItems_StockItems1] FOREIGN KEY([StockItemAID], [StockItemLID])
REFERENCES [dbo].[StockItems] ([StockItemAID], [StockItemLID])
ON UPDATE SET NULL
ON DELETE SET NULL
GO
ALTER TABLE [dbo].[StockOrderItems] CHECK CONSTRAINT [FK_StockOrderItems_StockItems1]
GO
ALTER TABLE [dbo].[StockOrderItems]  WITH CHECK ADD  CONSTRAINT [FK_StockOrderItems_StockOrders1] FOREIGN KEY([StockOrderAID], [StockOrderLID])
REFERENCES [dbo].[StockOrders] ([StockOrderAID], [StockOrderLID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[StockOrderItems] CHECK CONSTRAINT [FK_StockOrderItems_StockOrders1]
GO
ALTER TABLE [dbo].[StockOrderItems]  WITH CHECK ADD  CONSTRAINT [FK_StockOrderItems_Units] FOREIGN KEY([UnitAID], [UnitLID])
REFERENCES [dbo].[Units] ([UnitAID], [UnitLID])
GO
ALTER TABLE [dbo].[StockOrderItems] CHECK CONSTRAINT [FK_StockOrderItems_Units]
GO
ALTER TABLE [dbo].[StockOrders]  WITH CHECK ADD  CONSTRAINT [FK_StockOrders_Suppliers1] FOREIGN KEY([SupplierAID], [SupplierLID])
REFERENCES [dbo].[Suppliers] ([SupplierAID], [SupplierLID])
GO
ALTER TABLE [dbo].[StockOrders] CHECK CONSTRAINT [FK_StockOrders_Suppliers1]
GO
ALTER TABLE [dbo].[StockOrders]  WITH CHECK ADD  CONSTRAINT [FK_StockOrders_Vans] FOREIGN KEY([VanAID], [VanLID])
REFERENCES [dbo].[Vans] ([VanAID], [VanLID])
ON UPDATE SET NULL
ON DELETE SET NULL
GO
ALTER TABLE [dbo].[StockOrders] CHECK CONSTRAINT [FK_StockOrders_Vans]
GO
ALTER TABLE [dbo].[StockTake]  WITH NOCHECK ADD  CONSTRAINT [FK_StockTake_RevenueCentre] FOREIGN KEY([RevenueCentreAID], [RevenueCentreLID])
REFERENCES [dbo].[RevenueCentre] ([RevenueCentreAID], [RevenueCentreLID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[StockTake] CHECK CONSTRAINT [FK_StockTake_RevenueCentre]
GO
ALTER TABLE [dbo].[StockTakeItem]  WITH NOCHECK ADD  CONSTRAINT [FK_StockTakeItem_StockItems] FOREIGN KEY([StockItemAID], [StockItemLID])
REFERENCES [dbo].[StockItems] ([StockItemAID], [StockItemLID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[StockTakeItem] CHECK CONSTRAINT [FK_StockTakeItem_StockItems]
GO
ALTER TABLE [dbo].[StockTakeItem]  WITH NOCHECK ADD  CONSTRAINT [FK_StockTakeItem_StockTake] FOREIGN KEY([StockTakeAID], [StockTakeLID])
REFERENCES [dbo].[StockTake] ([StockTakeAID], [StockTakeLID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[StockTakeItem] CHECK CONSTRAINT [FK_StockTakeItem_StockTake]
GO
ALTER TABLE [dbo].[StockTransfers]  WITH CHECK ADD  CONSTRAINT [FK_StockTransfers_Locations] FOREIGN KEY([SendingLocationID])
REFERENCES [dbo].[Locations] ([LocationID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[StockTransfers] CHECK CONSTRAINT [FK_StockTransfers_Locations]
GO
ALTER TABLE [dbo].[StockTransfers]  WITH CHECK ADD  CONSTRAINT [FK_StockTransfers_RevenueCentre] FOREIGN KEY([SendingRevenueCentreAID], [SendingRevenueCentreLID])
REFERENCES [dbo].[RevenueCentre] ([RevenueCentreAID], [RevenueCentreLID])
GO
ALTER TABLE [dbo].[StockTransfers] CHECK CONSTRAINT [FK_StockTransfers_RevenueCentre]
GO
ALTER TABLE [dbo].[StockTransfers]  WITH CHECK ADD  CONSTRAINT [FK_StockTransfers_StockItems] FOREIGN KEY([StockItemAID], [StockItemLID])
REFERENCES [dbo].[StockItems] ([StockItemAID], [StockItemLID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[StockTransfers] CHECK CONSTRAINT [FK_StockTransfers_StockItems]
GO
ALTER TABLE [dbo].[StockTransfers]  WITH CHECK ADD  CONSTRAINT [FK_StockTransfers_Users] FOREIGN KEY([SendingUserAID], [SendingUserLID])
REFERENCES [dbo].[Users] ([UserAID], [UserLID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[StockTransfers] CHECK CONSTRAINT [FK_StockTransfers_Users]
GO
ALTER TABLE [dbo].[WebOrderDetails]  WITH CHECK ADD  CONSTRAINT [FK_WebOrderDetails_Orders] FOREIGN KEY([OrderAID], [OrderLID])
REFERENCES [dbo].[Orders] ([OrderAID], [OrderLID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[WebOrderDetails] CHECK CONSTRAINT [FK_WebOrderDetails_Orders]
GO
