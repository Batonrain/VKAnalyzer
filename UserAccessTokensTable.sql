USE [VKSMM]
GO

/****** Object:  Table [dbo].[UserAccessTokens]    Script Date: 01.04.2017 15:48:35 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[UserAccessTokens](
	[VkUserId] [nvarchar](128) NOT NULL,
	[AccessToken] [nvarchar](500) NOT NULL
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[UserAccessTokens]  WITH CHECK ADD  CONSTRAINT [FK_UserAccessTokens_AspNetUsers] FOREIGN KEY([VkUserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
GO

ALTER TABLE [dbo].[UserAccessTokens] CHECK CONSTRAINT [FK_UserAccessTokens_AspNetUsers]
GO


