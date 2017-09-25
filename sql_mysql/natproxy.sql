/*
Navicat MySQL Data Transfer

Source Server         : localhost_3306
Source Server Version : 50617
Source Host           : localhost:3306
Source Database       : myworld

Target Server Type    : MYSQL
Target Server Version : 50617
File Encoding         : 65001

Date: 2017-09-25 10:10:11
*/

SET FOREIGN_KEY_CHECKS=0;

-- ----------------------------
-- Table structure for `natproxy`
-- ----------------------------
DROP TABLE IF EXISTS `natproxy`;
CREATE TABLE `natproxy` (
  `Id` int(11) NOT NULL AUTO_INCREMENT COMMENT '主键Id',
  `Type` int(11) DEFAULT '0' COMMENT '类型（1：Nat，2：Proxy）',
  `AddressOuterNet` varchar(50) COLLATE utf8_unicode_ci DEFAULT NULL COMMENT '外网IpAddress',
  `AddressInnerNet` varchar(50) COLLATE utf8_unicode_ci DEFAULT NULL COMMENT '内网IpAddress',
  `Port` int(11) DEFAULT NULL COMMENT '端口',
  `StartUpTime` datetime DEFAULT NULL COMMENT '启动时间',
  `IsUsable` tinyint(1) DEFAULT NULL COMMENT '是否可用',
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_unicode_ci COMMENT='NatProxy 运行IPAddress 信息';

-- ----------------------------
-- Records of natproxy
-- ----------------------------
