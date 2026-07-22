CREATE DATABASE  IF NOT EXISTS `paired_db` /*!40100 DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci */ /*!80016 DEFAULT ENCRYPTION='N' */;
USE `paired_db`;
-- MySQL dump 10.13  Distrib 8.0.46, for Win64 (x86_64)
--
-- Host: localhost    Database: paired_db
-- ------------------------------------------------------
-- Server version	8.0.46

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!50503 SET NAMES utf8 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Table structure for table `feedback`
--

DROP TABLE IF EXISTS `feedback`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `feedback` (
  `FeedbackId` int NOT NULL AUTO_INCREMENT,
  `TutorId` int NOT NULL,
  `TuteeId` int NOT NULL,
  `SessionId` int NOT NULL,
  `Rating` int DEFAULT NULL,
  `Comment` text,
  PRIMARY KEY (`FeedbackId`),
  KEY `TutorId` (`TutorId`),
  KEY `TuteeId` (`TuteeId`),
  KEY `SessionId` (`SessionId`),
  CONSTRAINT `feedback_ibfk_1` FOREIGN KEY (`TutorId`) REFERENCES `users` (`UserId`),
  CONSTRAINT `feedback_ibfk_2` FOREIGN KEY (`TuteeId`) REFERENCES `users` (`UserId`),
  CONSTRAINT `feedback_ibfk_3` FOREIGN KEY (`SessionId`) REFERENCES `sessions` (`SessionId`),
  CONSTRAINT `feedback_chk_1` CHECK ((`Rating` between 1 and 5))
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `feedback`
--

LOCK TABLES `feedback` WRITE;
/*!40000 ALTER TABLE `feedback` DISABLE KEYS */;
/*!40000 ALTER TABLE `feedback` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `messages`
--

DROP TABLE IF EXISTS `messages`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `messages` (
  `MessageId` int NOT NULL AUTO_INCREMENT,
  `SenderId` int NOT NULL,
  `ReceiverId` int NOT NULL,
  `Content` text NOT NULL,
  `SentAt` datetime DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`MessageId`),
  KEY `SenderId` (`SenderId`),
  KEY `ReceiverId` (`ReceiverId`),
  CONSTRAINT `messages_ibfk_1` FOREIGN KEY (`SenderId`) REFERENCES `users` (`UserId`),
  CONSTRAINT `messages_ibfk_2` FOREIGN KEY (`ReceiverId`) REFERENCES `users` (`UserId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `messages`
--

LOCK TABLES `messages` WRITE;
/*!40000 ALTER TABLE `messages` DISABLE KEYS */;
/*!40000 ALTER TABLE `messages` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `requests`
--

DROP TABLE IF EXISTS `requests`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `requests` (
  `RequestId` int NOT NULL AUTO_INCREMENT,
  `TuteeId` int NOT NULL,
  `TutorId` int NOT NULL,
  `Subject` varchar(100) NOT NULL,
  `PreferredDate` datetime NOT NULL,
  `Status` enum('Pending','Accepted','Declined','Upcoming','Completed') DEFAULT 'Pending',
  `RejectionReason` text,
  PRIMARY KEY (`RequestId`),
  KEY `TuteeId` (`TuteeId`),
  KEY `TutorId` (`TutorId`),
  CONSTRAINT `requests_ibfk_1` FOREIGN KEY (`TuteeId`) REFERENCES `users` (`UserId`),
  CONSTRAINT `requests_ibfk_2` FOREIGN KEY (`TutorId`) REFERENCES `users` (`UserId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `requests`
--

LOCK TABLES `requests` WRITE;
/*!40000 ALTER TABLE `requests` DISABLE KEYS */;
/*!40000 ALTER TABLE `requests` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `sessions`
--

DROP TABLE IF EXISTS `sessions`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `sessions` (
  `SessionId` int NOT NULL AUTO_INCREMENT,
  `RequestId` int NOT NULL,
  `TutorId` int NOT NULL,
  `StudentId` int NOT NULL,
  `Subject` varchar(100) NOT NULL,
  `ScheduledTime` datetime NOT NULL,
  `Status` varchar(20) DEFAULT 'Pending',
  PRIMARY KEY (`SessionId`),
  KEY `RequestId` (`RequestId`),
  CONSTRAINT `sessions_ibfk_1` FOREIGN KEY (`RequestId`) REFERENCES `requests` (`RequestId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `sessions`
--

LOCK TABLES `sessions` WRITE;
/*!40000 ALTER TABLE `sessions` DISABLE KEYS */;
/*!40000 ALTER TABLE `sessions` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `users`
--

DROP TABLE IF EXISTS `users`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `users` (
  `UserId` int NOT NULL AUTO_INCREMENT,
  `Email` varchar(255) NOT NULL,
  `PasswordHash` varchar(255) NOT NULL,
  `DisplayName` varchar(100) NOT NULL,
  `ProfilePicture` varchar(255) DEFAULT NULL,
  `IsTutor` tinyint(1) DEFAULT '0',
  `Availability` varchar(255) DEFAULT NULL,
  `Role` varchar(50) NOT NULL DEFAULT 'Student',
  PRIMARY KEY (`UserId`),
  UNIQUE KEY `Email` (`Email`)
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `users`
--

LOCK TABLES `users` WRITE;
/*!40000 ALTER TABLE `users` DISABLE KEYS */;
INSERT INTO `users` VALUES (1,'lysie@example.com','mypassword','Lysie',NULL,0,NULL,'Student'),(3,'student1@example.com','AQAAAAIAAYagAAAAEKvA3EGDqKglduZ0Gkc0Ha2nZDsnVBW0Acqa3rT97AZIWOJcNl2DtDEChOTHokKMiQ==','Student One',NULL,0,'Weekdays 6-9 PM','Student');
/*!40000 ALTER TABLE `users` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `userskills`
--

DROP TABLE IF EXISTS `userskills`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `userskills` (
  `SkillId` int NOT NULL AUTO_INCREMENT,
  `UserId` int NOT NULL,
  `SkillName` varchar(100) NOT NULL,
  PRIMARY KEY (`SkillId`),
  KEY `UserId` (`UserId`),
  CONSTRAINT `userskills_ibfk_1` FOREIGN KEY (`UserId`) REFERENCES `users` (`UserId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `userskills`
--

LOCK TABLES `userskills` WRITE;
/*!40000 ALTER TABLE `userskills` DISABLE KEYS */;
/*!40000 ALTER TABLE `userskills` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2026-07-22 23:22:57
