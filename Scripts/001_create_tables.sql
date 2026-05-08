-- Run this script against your MySQL database before starting the application.

-- Membership table (already exists in your DB; included here for reference)
CREATE TABLE IF NOT EXISTS `tm_membership` (
  `MEMBER_ID`    int(11)      NOT NULL AUTO_INCREMENT,
  `MEMBERSHIP_NO` varchar(30) DEFAULT NULL,
  `FORM_NUMBER`  int(11)      DEFAULT NULL,
  `FORM_DATE`    date         DEFAULT NULL,
  `NAME`         varchar(150) DEFAULT NULL,
  `FATHER_NAME`  varchar(150) DEFAULT NULL,
  `ADDRESS`      varchar(150) DEFAULT NULL,
  `PhoneNo`      varchar(20)  DEFAULT NULL,
  `CreatedDate`  date         DEFAULT NULL,
  `Age`          double       DEFAULT '0',
  `Birthdate`    date         DEFAULT NULL,
  `Pancard`      varchar(45)  DEFAULT NULL,
  `AdharCard`    varchar(45)  DEFAULT NULL,
  `AuthStatus`   varchar(1)   DEFAULT 'U',
  PRIMARY KEY (`MEMBER_ID`),
  KEY `index2` (`MEMBERSHIP_NO`),
  KEY `idx_phone` (`PhoneNo`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- OTP table for persistent OTP storage (survives restarts, works with multiple servers)
CREATE TABLE IF NOT EXISTS `tm_otp` (
  `ID`        bigint       NOT NULL AUTO_INCREMENT,
  `PhoneNo`   varchar(20)  NOT NULL,
  `OtpCode`   varchar(6)   NOT NULL,
  `CreatedAt` datetime     NOT NULL,
  `ExpiresAt` datetime     NOT NULL,
  `IsUsed`    tinyint(1)   NOT NULL DEFAULT '0',
  PRIMARY KEY (`ID`),
  KEY `idx_otp_phone` (`PhoneNo`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
