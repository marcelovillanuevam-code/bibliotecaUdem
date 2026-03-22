/*M!999999\- enable the sandbox mode */ 
-- MariaDB dump 10.19-11.7.2-MariaDB, for Win64 (AMD64)
--
-- Host: localhost    Database: library_mgmt
-- ------------------------------------------------------
-- Server version	12.0.2-MariaDB

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*M!100616 SET @OLD_NOTE_VERBOSITY=@@NOTE_VERBOSITY, NOTE_VERBOSITY=0 */;

--
-- Table structure for table `audit_logs`
--

DROP TABLE IF EXISTS `audit_logs`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `audit_logs` (
  `id` char(36) NOT NULL,
  `table_name` varchar(128) NOT NULL,
  `record_id` char(36) DEFAULT NULL,
  `action` enum('INSERT','UPDATE','DELETE') NOT NULL,
  `performed_by` char(36) DEFAULT NULL,
  `performed_at` timestamp NOT NULL DEFAULT current_timestamp(),
  `changed_data` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_bin DEFAULT NULL CHECK (json_valid(`changed_data`)),
  `reason` text DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `idx_audit_performed_at` (`performed_at`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_uca1400_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `audit_logs`
--

LOCK TABLES `audit_logs` WRITE;
/*!40000 ALTER TABLE `audit_logs` DISABLE KEYS */;
INSERT INTO `audit_logs` VALUES
('5baf38a2-23c0-11f1-b6ca-1cce51c6125a','users','5ba80dcd-23c0-11f1-b6ca-1cce51c6125a','INSERT',NULL,'2026-03-19 18:20:25','{\"new\": {\"id\": \"5ba80dcd-23c0-11f1-b6ca-1cce51c6125a\", \"username\": \"admin1\", \"status\": \"active\", \"created_at\": \"2026-03-19 12:20:25\"}}',NULL),
('5bb40e6d-23c0-11f1-b6ca-1cce51c6125a','user_profiles','5ba80dcd-23c0-11f1-b6ca-1cce51c6125a','INSERT',NULL,'2026-03-19 18:20:25','{\"new\": {\"first_name\": \"Admin\", \"last_name\": \"One\"}}',NULL),
('5bbadfb4-23c0-11f1-b6ca-1cce51c6125a','user_contacts','5bbacf98-23c0-11f1-b6ca-1cce51c6125a','INSERT',NULL,'2026-03-19 18:20:25','{\"new\": {\"type\": \"email\", \"value\": \"admin@example.com\"}}',NULL),
('5bbe0e9d-23c0-11f1-b6ca-1cce51c6125a','user_roles','5ba80dcd-23c0-11f1-b6ca-1cce51c6125a','INSERT',NULL,'2026-03-19 18:20:25','{\"new\": {\"role_id\": \"5ba30193-23c0-11f1-b6ca-1cce51c6125a\", \"assigned_at\": \"2026-03-19 12:20:25\"}}',NULL),
('7d287e0c-23c1-11f1-b6ca-1cce51c6125a','books','7d287354-23c1-11f1-b6ca-1cce51c6125a','INSERT',NULL,'2026-03-19 18:28:31','{\"new\": {\"title\": \"Cien años de soledad\", \"isbn\": \"9780307474728\"}}',NULL),
('7d28858a-23c1-11f1-b6ca-1cce51c6125a','books','7d288140-23c1-11f1-b6ca-1cce51c6125a','INSERT',NULL,'2026-03-19 18:28:31','{\"new\": {\"title\": \"Fundación\", \"isbn\": \"9780553293357\"}}',NULL),
('7d28891e-23c1-11f1-b6ca-1cce51c6125a','books','7d288711-23c1-11f1-b6ca-1cce51c6125a','INSERT',NULL,'2026-03-19 18:28:31','{\"new\": {\"title\": \"Pride and Prejudice\", \"isbn\": \"9780141199078\"}}',NULL);
/*!40000 ALTER TABLE `audit_logs` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `authors`
--

DROP TABLE IF EXISTS `authors`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `authors` (
  `id` char(36) NOT NULL,
  `full_name` varchar(255) NOT NULL,
  `birth_date` date DEFAULT NULL,
  `bio` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_bin DEFAULT NULL CHECK (json_valid(`bio`)),
  `created_at` timestamp NOT NULL DEFAULT current_timestamp(),
  `updated_at` timestamp NOT NULL DEFAULT current_timestamp() ON UPDATE current_timestamp(),
  PRIMARY KEY (`id`),
  KEY `idx_authors_name` (`full_name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_uca1400_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `authors`
--

LOCK TABLES `authors` WRITE;
/*!40000 ALTER TABLE `authors` DISABLE KEYS */;
INSERT INTO `authors` VALUES
('7d235a4e-23c1-11f1-b6ca-1cce51c6125a','Gabriel García Márquez','1927-03-06','{\"notes\": \"Realismo mágico\"}','2026-03-19 18:28:31','2026-03-19 18:28:31'),
('7d23604a-23c1-11f1-b6ca-1cce51c6125a','Isaac Asimov','1920-01-02','{\"notes\": \"Ciencia ficción\"}','2026-03-19 18:28:31','2026-03-19 18:28:31'),
('7d23d4b8-23c1-11f1-b6ca-1cce51c6125a','Jane Austen','1775-12-16','{\"notes\": \"Novela clásica\"}','2026-03-19 18:28:31','2026-03-19 18:28:31'),
('870ed64c-23c1-11f1-b6ca-1cce51c6125a','Gabriel García Márquez','1927-03-06','{\"notes\": \"Realismo mágico\"}','2026-03-19 18:28:47','2026-03-19 18:28:47'),
('870edb46-23c1-11f1-b6ca-1cce51c6125a','Isaac Asimov','1920-01-02','{\"notes\": \"Ciencia ficción\"}','2026-03-19 18:28:47','2026-03-19 18:28:47'),
('870edcd5-23c1-11f1-b6ca-1cce51c6125a','Jane Austen','1775-12-16','{\"notes\": \"Novela clásica\"}','2026-03-19 18:28:47','2026-03-19 18:28:47');
/*!40000 ALTER TABLE `authors` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `book_authors`
--

DROP TABLE IF EXISTS `book_authors`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `book_authors` (
  `book_id` char(36) NOT NULL,
  `author_id` char(36) NOT NULL,
  `contribution` varchar(100) DEFAULT NULL,
  PRIMARY KEY (`book_id`,`author_id`),
  KEY `fk_ba_author` (`author_id`),
  CONSTRAINT `fk_ba_author` FOREIGN KEY (`author_id`) REFERENCES `authors` (`id`),
  CONSTRAINT `fk_ba_book` FOREIGN KEY (`book_id`) REFERENCES `books` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_uca1400_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `book_authors`
--

LOCK TABLES `book_authors` WRITE;
/*!40000 ALTER TABLE `book_authors` DISABLE KEYS */;
INSERT INTO `book_authors` VALUES
('7d287354-23c1-11f1-b6ca-1cce51c6125a','7d235a4e-23c1-11f1-b6ca-1cce51c6125a',NULL),
('7d288140-23c1-11f1-b6ca-1cce51c6125a','7d23604a-23c1-11f1-b6ca-1cce51c6125a',NULL),
('7d288711-23c1-11f1-b6ca-1cce51c6125a','7d23d4b8-23c1-11f1-b6ca-1cce51c6125a',NULL);
/*!40000 ALTER TABLE `book_authors` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `book_subjects`
--

DROP TABLE IF EXISTS `book_subjects`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `book_subjects` (
  `book_id` char(36) NOT NULL,
  `subject_id` char(36) NOT NULL,
  PRIMARY KEY (`book_id`,`subject_id`),
  KEY `fk_bs_subject` (`subject_id`),
  CONSTRAINT `fk_bs_book` FOREIGN KEY (`book_id`) REFERENCES `books` (`id`) ON DELETE CASCADE,
  CONSTRAINT `fk_bs_subject` FOREIGN KEY (`subject_id`) REFERENCES `subjects` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_uca1400_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `book_subjects`
--

LOCK TABLES `book_subjects` WRITE;
/*!40000 ALTER TABLE `book_subjects` DISABLE KEYS */;
INSERT INTO `book_subjects` VALUES
('7d287354-23c1-11f1-b6ca-1cce51c6125a','7d24c19b-23c1-11f1-b6ca-1cce51c6125a'),
('7d288140-23c1-11f1-b6ca-1cce51c6125a','7d24c4ff-23c1-11f1-b6ca-1cce51c6125a'),
('7d288711-23c1-11f1-b6ca-1cce51c6125a','7d24c7c0-23c1-11f1-b6ca-1cce51c6125a');
/*!40000 ALTER TABLE `book_subjects` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `books`
--

DROP TABLE IF EXISTS `books`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `books` (
  `id` char(36) NOT NULL,
  `title` varchar(500) NOT NULL,
  `subtitle` varchar(500) DEFAULT NULL,
  `isbn` varchar(50) DEFAULT NULL,
  `publisher` varchar(255) DEFAULT NULL,
  `publication_year` smallint(6) DEFAULT NULL,
  `edition` varchar(100) DEFAULT NULL,
  `language` varchar(50) DEFAULT 'es',
  `summary` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_bin DEFAULT NULL CHECK (json_valid(`summary`)),
  `metadata` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_bin DEFAULT NULL CHECK (json_valid(`metadata`)),
  `created_at` timestamp NOT NULL DEFAULT current_timestamp(),
  `updated_at` timestamp NOT NULL DEFAULT current_timestamp() ON UPDATE current_timestamp(),
  PRIMARY KEY (`id`),
  UNIQUE KEY `ux_books_isbn` (`isbn`),
  KEY `idx_books_title` (`title`(255)),
  KEY `idx_books_isbn` (`isbn`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_uca1400_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `books`
--

LOCK TABLES `books` WRITE;
/*!40000 ALTER TABLE `books` DISABLE KEYS */;
INSERT INTO `books` VALUES
('7d287354-23c1-11f1-b6ca-1cce51c6125a','Cien años de soledad',NULL,'9780307474728','Editorial Sudamericana',1967,'1','es','{\"short\": \"Saga familiar en Macondo\"}','{\"format\": \"tapa dura\"}','2026-03-19 18:28:31','2026-03-19 18:28:31'),
('7d288140-23c1-11f1-b6ca-1cce51c6125a','Fundación',NULL,'9780553293357','Gnome Press',1951,'1','es','{\"short\": \"Saga galáctica sobre psicohistoria\"}','{\"format\": \"tapa blanda\"}','2026-03-19 18:28:31','2026-03-19 18:28:31'),
('7d288711-23c1-11f1-b6ca-1cce51c6125a','Pride and Prejudice','A Novel','9780141199078','T. Egerton',1813,'1','en','{\"short\": \"Novela sobre costumbres y matrimonio\"}','{\"format\": \"tapa blanda\"}','2026-03-19 18:28:31','2026-03-19 18:28:31');
/*!40000 ALTER TABLE `books` ENABLE KEYS */;
UNLOCK TABLES;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_uca1400_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'IGNORE_SPACE,STRICT_TRANS_TABLES,ERROR_FOR_DIVISION_BY_ZERO,NO_AUTO_CREATE_USER,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
/*!50003 CREATE*/ /*!50017 DEFINER=`root`@`localhost`*/ /*!50003 TRIGGER trg_audit_books_insert AFTER INSERT ON books FOR EACH ROW
BEGIN
  INSERT INTO audit_logs (id, table_name, record_id, action, performed_by, changed_data)
  VALUES (UUID(), 'books', NEW.id, 'INSERT', @app_current_user, JSON_OBJECT('new', JSON_OBJECT('title', NEW.title, 'isbn', NEW.isbn)));
END */;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_uca1400_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'IGNORE_SPACE,STRICT_TRANS_TABLES,ERROR_FOR_DIVISION_BY_ZERO,NO_AUTO_CREATE_USER,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
/*!50003 CREATE*/ /*!50017 DEFINER=`root`@`localhost`*/ /*!50003 TRIGGER trg_audit_books_update AFTER UPDATE ON books FOR EACH ROW
BEGIN
  INSERT INTO audit_logs (id, table_name, record_id, action, performed_by, changed_data)
  VALUES (UUID(), 'books', NEW.id, 'UPDATE', @app_current_user, JSON_OBJECT('old', JSON_OBJECT('title', OLD.title, 'isbn', OLD.isbn), 'new', JSON_OBJECT('title', NEW.title, 'isbn', NEW.isbn)));
END */;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_uca1400_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'IGNORE_SPACE,STRICT_TRANS_TABLES,ERROR_FOR_DIVISION_BY_ZERO,NO_AUTO_CREATE_USER,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
/*!50003 CREATE*/ /*!50017 DEFINER=`root`@`localhost`*/ /*!50003 TRIGGER trg_audit_books_delete AFTER DELETE ON books FOR EACH ROW
BEGIN
  INSERT INTO audit_logs (id, table_name, record_id, action, performed_by, changed_data)
  VALUES (UUID(), 'books', OLD.id, 'DELETE', @app_current_user, JSON_OBJECT('old', JSON_OBJECT('title', OLD.title, 'isbn', OLD.isbn)));
END */;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;

--
-- Table structure for table `locations`
--

DROP TABLE IF EXISTS `locations`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `locations` (
  `id` char(36) NOT NULL,
  `library_name` varchar(200) NOT NULL,
  `section` varchar(100) DEFAULT NULL,
  `shelf` varchar(100) DEFAULT NULL,
  `notes` varchar(255) DEFAULT NULL,
  `created_at` timestamp NOT NULL DEFAULT current_timestamp(),
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_uca1400_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `locations`
--

LOCK TABLES `locations` WRITE;
/*!40000 ALTER TABLE `locations` DISABLE KEYS */;
INSERT INTO `locations` VALUES
('7d25b5aa-23c1-11f1-b6ca-1cce51c6125a','Biblioteca Central','Humanidades','A1','Estantería principal Humanidades','2026-03-19 18:28:31'),
('7d25bc4e-23c1-11f1-b6ca-1cce51c6125a','Biblioteca Central','Ciencias','C3','Estantería Ciencia y Tecnología','2026-03-19 18:28:31');
/*!40000 ALTER TABLE `locations` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `password_reset_tokens`
--

DROP TABLE IF EXISTS `password_reset_tokens`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `password_reset_tokens` (
  `id` char(36) NOT NULL,
  `user_id` char(36) NOT NULL,
  `token` varchar(255) NOT NULL,
  `created_at` timestamp NOT NULL DEFAULT current_timestamp(),
  `expires_at` timestamp NOT NULL,
  `used_at` timestamp NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `token` (`token`),
  KEY `fk_prt_user` (`user_id`),
  CONSTRAINT `fk_prt_user` FOREIGN KEY (`user_id`) REFERENCES `users` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_uca1400_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `password_reset_tokens`
--

LOCK TABLES `password_reset_tokens` WRITE;
/*!40000 ALTER TABLE `password_reset_tokens` DISABLE KEYS */;
/*!40000 ALTER TABLE `password_reset_tokens` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `roles`
--

DROP TABLE IF EXISTS `roles`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `roles` (
  `id` char(36) NOT NULL,
  `code` varchar(50) NOT NULL,
  `display_name` varchar(100) NOT NULL,
  `description` text DEFAULT NULL,
  `created_at` timestamp NOT NULL DEFAULT current_timestamp(),
  PRIMARY KEY (`id`),
  UNIQUE KEY `code` (`code`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_uca1400_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `roles`
--

LOCK TABLES `roles` WRITE;
/*!40000 ALTER TABLE `roles` DISABLE KEYS */;
INSERT INTO `roles` VALUES
('5ba2f895-23c0-11f1-b6ca-1cce51c6125a','STUDENT','Student','Estudiante','2026-03-19 18:20:25'),
('5ba2ff3e-23c0-11f1-b6ca-1cce51c6125a','LIBRARIAN','Librarian','Bibliotecario','2026-03-19 18:20:25'),
('5ba30106-23c0-11f1-b6ca-1cce51c6125a','TEACHER','Teacher','Profesor','2026-03-19 18:20:25'),
('5ba30193-23c0-11f1-b6ca-1cce51c6125a','ADMIN','Administrator','Administrador','2026-03-19 18:20:25');
/*!40000 ALTER TABLE `roles` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `sessions`
--

DROP TABLE IF EXISTS `sessions`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `sessions` (
  `id` char(36) NOT NULL,
  `user_id` char(36) NOT NULL,
  `session_token` varchar(255) NOT NULL,
  `ip_address` varchar(45) DEFAULT NULL,
  `user_agent` varchar(512) DEFAULT NULL,
  `created_at` timestamp NOT NULL DEFAULT current_timestamp(),
  `expires_at` timestamp NOT NULL,
  `revoked_at` timestamp NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `session_token` (`session_token`),
  KEY `idx_sessions_userid` (`user_id`),
  CONSTRAINT `fk_sessions_user` FOREIGN KEY (`user_id`) REFERENCES `users` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_uca1400_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `sessions`
--

LOCK TABLES `sessions` WRITE;
/*!40000 ALTER TABLE `sessions` DISABLE KEYS */;
/*!40000 ALTER TABLE `sessions` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `statuses`
--

DROP TABLE IF EXISTS `statuses`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `statuses` (
  `code` varchar(30) NOT NULL,
  `description` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`code`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_uca1400_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `statuses`
--

LOCK TABLES `statuses` WRITE;
/*!40000 ALTER TABLE `statuses` DISABLE KEYS */;
INSERT INTO `statuses` VALUES
('active','Activo'),
('deleted','Eliminado'),
('inactive','Inactivo'),
('pending_verification','Pendiente de verificación'),
('suspended','Suspendido');
/*!40000 ALTER TABLE `statuses` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `subjects`
--

DROP TABLE IF EXISTS `subjects`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `subjects` (
  `id` char(36) NOT NULL,
  `code` varchar(100) NOT NULL,
  `name` varchar(255) NOT NULL,
  `description` text DEFAULT NULL,
  `created_at` timestamp NOT NULL DEFAULT current_timestamp(),
  PRIMARY KEY (`id`),
  UNIQUE KEY `code` (`code`),
  KEY `idx_subjects_name` (`name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_uca1400_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `subjects`
--

LOCK TABLES `subjects` WRITE;
/*!40000 ALTER TABLE `subjects` DISABLE KEYS */;
INSERT INTO `subjects` VALUES
('7d24c19b-23c1-11f1-b6ca-1cce51c6125a','LIT_LATAM','Literatura Latinoamericana','Obras de autores latinoamericanos','2026-03-19 18:28:31'),
('7d24c4ff-23c1-11f1-b6ca-1cce51c6125a','SCI_FI','Ciencia Ficción','Obras de ciencia ficción','2026-03-19 18:28:31'),
('7d24c7c0-23c1-11f1-b6ca-1cce51c6125a','LIT_CLASSIC','Literatura Clásica','Obras clásicas de la literatura','2026-03-19 18:28:31');
/*!40000 ALTER TABLE `subjects` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `user_auth`
--

DROP TABLE IF EXISTS `user_auth`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `user_auth` (
  `user_id` char(36) NOT NULL,
  `password_hash` varchar(255) NOT NULL,
  `password_changed_at` timestamp NULL DEFAULT NULL,
  `mfa_enabled` tinyint(1) NOT NULL DEFAULT 0,
  `mfa_secret` varbinary(512) DEFAULT NULL,
  `failed_login_count` int(11) NOT NULL DEFAULT 0,
  `locked_until` timestamp NULL DEFAULT NULL,
  `created_at` timestamp NOT NULL DEFAULT current_timestamp(),
  `updated_at` timestamp NOT NULL DEFAULT current_timestamp() ON UPDATE current_timestamp(),
  PRIMARY KEY (`user_id`),
  CONSTRAINT `fk_auth_user` FOREIGN KEY (`user_id`) REFERENCES `users` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_uca1400_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `user_auth`
--

LOCK TABLES `user_auth` WRITE;
/*!40000 ALTER TABLE `user_auth` DISABLE KEYS */;
INSERT INTO `user_auth` VALUES
('5ba80dcd-23c0-11f1-b6ca-1cce51c6125a','$2y$12$PLACEHOLDER_FOR_BCRYPT_HASH_GENERATED_BY_APP','2026-03-19 18:20:25',0,NULL,0,NULL,'2026-03-19 18:20:25','2026-03-19 18:20:25');
/*!40000 ALTER TABLE `user_auth` ENABLE KEYS */;
UNLOCK TABLES;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_uca1400_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'IGNORE_SPACE,STRICT_TRANS_TABLES,ERROR_FOR_DIVISION_BY_ZERO,NO_AUTO_CREATE_USER,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
/*!50003 CREATE*/ /*!50017 DEFINER=`root`@`localhost`*/ /*!50003 TRIGGER trg_audit_user_auth_update AFTER UPDATE ON user_auth FOR EACH ROW BEGIN INSERT INTO audit_logs (id, table_name, record_id, action, performed_by, changed_data) VALUES (UUID(), 'user_auth', NEW.user_id, 'UPDATE', @app_current_user, JSON_OBJECT('old', JSON_OBJECT('failed_login_count', OLD.failed_login_count), 'new', JSON_OBJECT('failed_login_count', NEW.failed_login_count))); END */;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;

--
-- Table structure for table `user_contacts`
--

DROP TABLE IF EXISTS `user_contacts`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `user_contacts` (
  `id` char(36) NOT NULL,
  `user_id` char(36) NOT NULL,
  `type` enum('email','phone') NOT NULL,
  `value` varchar(255) NOT NULL,
  `is_primary` tinyint(1) NOT NULL DEFAULT 0,
  `is_verified` tinyint(1) NOT NULL DEFAULT 0,
  `verified_at` timestamp NULL DEFAULT NULL,
  `created_at` timestamp NOT NULL DEFAULT current_timestamp(),
  `updated_at` timestamp NOT NULL DEFAULT current_timestamp() ON UPDATE current_timestamp(),
  PRIMARY KEY (`id`),
  UNIQUE KEY `ux_contact_type_value` (`type`,`value`),
  KEY `idx_user_contacts_userid` (`user_id`),
  CONSTRAINT `fk_contacts_user` FOREIGN KEY (`user_id`) REFERENCES `users` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_uca1400_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `user_contacts`
--

LOCK TABLES `user_contacts` WRITE;
/*!40000 ALTER TABLE `user_contacts` DISABLE KEYS */;
INSERT INTO `user_contacts` VALUES
('5bbacf98-23c0-11f1-b6ca-1cce51c6125a','5ba80dcd-23c0-11f1-b6ca-1cce51c6125a','email','admin@example.com',1,0,NULL,'2026-03-19 18:20:25','2026-03-19 18:20:25');
/*!40000 ALTER TABLE `user_contacts` ENABLE KEYS */;
UNLOCK TABLES;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_uca1400_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'IGNORE_SPACE,STRICT_TRANS_TABLES,ERROR_FOR_DIVISION_BY_ZERO,NO_AUTO_CREATE_USER,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
/*!50003 CREATE*/ /*!50017 DEFINER=`root`@`localhost`*/ /*!50003 TRIGGER trg_audit_contacts_insert AFTER INSERT ON user_contacts FOR EACH ROW BEGIN INSERT INTO audit_logs (id, table_name, record_id, action, performed_by, changed_data) VALUES (UUID(), 'user_contacts', NEW.id, 'INSERT', @app_current_user, JSON_OBJECT('new', JSON_OBJECT('type', NEW.type, 'value', NEW.value))); END */;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_uca1400_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'IGNORE_SPACE,STRICT_TRANS_TABLES,ERROR_FOR_DIVISION_BY_ZERO,NO_AUTO_CREATE_USER,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
/*!50003 CREATE*/ /*!50017 DEFINER=`root`@`localhost`*/ /*!50003 TRIGGER trg_audit_contacts_update AFTER UPDATE ON user_contacts FOR EACH ROW BEGIN INSERT INTO audit_logs (id, table_name, record_id, action, performed_by, changed_data) VALUES (UUID(), 'user_contacts', NEW.id, 'UPDATE', @app_current_user, JSON_OBJECT('old', JSON_OBJECT('value', OLD.value, 'is_verified', OLD.is_verified), 'new', JSON_OBJECT('value', NEW.value, 'is_verified', NEW.is_verified))); END */;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_uca1400_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'IGNORE_SPACE,STRICT_TRANS_TABLES,ERROR_FOR_DIVISION_BY_ZERO,NO_AUTO_CREATE_USER,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
/*!50003 CREATE*/ /*!50017 DEFINER=`root`@`localhost`*/ /*!50003 TRIGGER trg_audit_contacts_delete AFTER DELETE ON user_contacts FOR EACH ROW BEGIN INSERT INTO audit_logs (id, table_name, record_id, action, performed_by, changed_data) VALUES (UUID(), 'user_contacts', OLD.id, 'DELETE', @app_current_user, JSON_OBJECT('old', JSON_OBJECT('type', OLD.type, 'value', OLD.value))); END */;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;

--
-- Table structure for table `user_profiles`
--

DROP TABLE IF EXISTS `user_profiles`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `user_profiles` (
  `user_id` char(36) NOT NULL,
  `first_name` varchar(100) NOT NULL,
  `last_name` varchar(100) NOT NULL,
  `display_name` varchar(200) DEFAULT NULL,
  `document_type` varchar(50) DEFAULT NULL,
  `document_number` varchar(100) DEFAULT NULL,
  `birth_date` date DEFAULT NULL,
  `gender` varchar(30) DEFAULT NULL,
  `address` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_bin DEFAULT NULL CHECK (json_valid(`address`)),
  `created_at` timestamp NOT NULL DEFAULT current_timestamp(),
  `updated_at` timestamp NOT NULL DEFAULT current_timestamp() ON UPDATE current_timestamp(),
  PRIMARY KEY (`user_id`),
  CONSTRAINT `fk_profiles_user` FOREIGN KEY (`user_id`) REFERENCES `users` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_uca1400_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `user_profiles`
--

LOCK TABLES `user_profiles` WRITE;
/*!40000 ALTER TABLE `user_profiles` DISABLE KEYS */;
INSERT INTO `user_profiles` VALUES
('5ba80dcd-23c0-11f1-b6ca-1cce51c6125a','Admin','One','Admin One',NULL,NULL,NULL,NULL,NULL,'2026-03-19 18:20:25','2026-03-19 18:20:25');
/*!40000 ALTER TABLE `user_profiles` ENABLE KEYS */;
UNLOCK TABLES;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_uca1400_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'IGNORE_SPACE,STRICT_TRANS_TABLES,ERROR_FOR_DIVISION_BY_ZERO,NO_AUTO_CREATE_USER,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
/*!50003 CREATE*/ /*!50017 DEFINER=`root`@`localhost`*/ /*!50003 TRIGGER trg_audit_profiles_insert AFTER INSERT ON user_profiles FOR EACH ROW BEGIN INSERT INTO audit_logs (id, table_name, record_id, action, performed_by, changed_data) VALUES (UUID(), 'user_profiles', NEW.user_id, 'INSERT', @app_current_user, JSON_OBJECT('new', JSON_OBJECT('first_name', NEW.first_name, 'last_name', NEW.last_name))); END */;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_uca1400_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'IGNORE_SPACE,STRICT_TRANS_TABLES,ERROR_FOR_DIVISION_BY_ZERO,NO_AUTO_CREATE_USER,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
/*!50003 CREATE*/ /*!50017 DEFINER=`root`@`localhost`*/ /*!50003 TRIGGER trg_audit_profiles_update AFTER UPDATE ON user_profiles FOR EACH ROW BEGIN INSERT INTO audit_logs (id, table_name, record_id, action, performed_by, changed_data) VALUES (UUID(), 'user_profiles', NEW.user_id, 'UPDATE', @app_current_user, JSON_OBJECT('old', JSON_OBJECT('first_name', OLD.first_name, 'last_name', OLD.last_name), 'new', JSON_OBJECT('first_name', NEW.first_name, 'last_name', NEW.last_name))); END */;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_uca1400_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'IGNORE_SPACE,STRICT_TRANS_TABLES,ERROR_FOR_DIVISION_BY_ZERO,NO_AUTO_CREATE_USER,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
/*!50003 CREATE*/ /*!50017 DEFINER=`root`@`localhost`*/ /*!50003 TRIGGER trg_audit_profiles_delete AFTER DELETE ON user_profiles FOR EACH ROW BEGIN INSERT INTO audit_logs (id, table_name, record_id, action, performed_by, changed_data) VALUES (UUID(), 'user_profiles', OLD.user_id, 'DELETE', @app_current_user, JSON_OBJECT('old', JSON_OBJECT('first_name', OLD.first_name, 'last_name', OLD.last_name))); END */;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;

--
-- Table structure for table `user_roles`
--

DROP TABLE IF EXISTS `user_roles`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `user_roles` (
  `user_id` char(36) NOT NULL,
  `role_id` char(36) NOT NULL,
  `assigned_by` char(36) DEFAULT NULL,
  `assigned_at` timestamp NOT NULL DEFAULT current_timestamp(),
  PRIMARY KEY (`user_id`,`role_id`),
  KEY `idx_user_roles_roleid` (`role_id`),
  CONSTRAINT `fk_userroles_role` FOREIGN KEY (`role_id`) REFERENCES `roles` (`id`),
  CONSTRAINT `fk_userroles_user` FOREIGN KEY (`user_id`) REFERENCES `users` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_uca1400_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `user_roles`
--

LOCK TABLES `user_roles` WRITE;
/*!40000 ALTER TABLE `user_roles` DISABLE KEYS */;
INSERT INTO `user_roles` VALUES
('5ba80dcd-23c0-11f1-b6ca-1cce51c6125a','5ba30193-23c0-11f1-b6ca-1cce51c6125a',NULL,'2026-03-19 18:20:25');
/*!40000 ALTER TABLE `user_roles` ENABLE KEYS */;
UNLOCK TABLES;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_uca1400_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'IGNORE_SPACE,STRICT_TRANS_TABLES,ERROR_FOR_DIVISION_BY_ZERO,NO_AUTO_CREATE_USER,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
/*!50003 CREATE*/ /*!50017 DEFINER=`root`@`localhost`*/ /*!50003 TRIGGER trg_audit_user_roles_insert AFTER INSERT ON user_roles FOR EACH ROW BEGIN INSERT INTO audit_logs (id, table_name, record_id, action, performed_by, changed_data) VALUES (UUID(), 'user_roles', NEW.user_id, 'INSERT', @app_current_user, JSON_OBJECT('new', JSON_OBJECT('role_id', NEW.role_id, 'assigned_at', NEW.assigned_at))); END */;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_uca1400_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'IGNORE_SPACE,STRICT_TRANS_TABLES,ERROR_FOR_DIVISION_BY_ZERO,NO_AUTO_CREATE_USER,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
/*!50003 CREATE*/ /*!50017 DEFINER=`root`@`localhost`*/ /*!50003 TRIGGER trg_audit_user_roles_delete AFTER DELETE ON user_roles FOR EACH ROW BEGIN INSERT INTO audit_logs (id, table_name, record_id, action, performed_by, changed_data) VALUES (UUID(), 'user_roles', OLD.user_id, 'DELETE', @app_current_user, JSON_OBJECT('old', JSON_OBJECT('role_id', OLD.role_id))); END */;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;

--
-- Table structure for table `users`
--

DROP TABLE IF EXISTS `users`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8mb4 */;
CREATE TABLE `users` (
  `id` char(36) NOT NULL,
  `username` varchar(100) NOT NULL,
  `username_lower` varchar(100) GENERATED ALWAYS AS (lcase(`username`)) STORED,
  `status` varchar(30) NOT NULL DEFAULT 'pending_verification',
  `preferred_locale` varchar(10) DEFAULT 'es_MX',
  `metadata` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_bin DEFAULT NULL CHECK (json_valid(`metadata`)),
  `created_at` timestamp NOT NULL DEFAULT current_timestamp(),
  `updated_at` timestamp NOT NULL DEFAULT current_timestamp() ON UPDATE current_timestamp(),
  `deleted_at` timestamp NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `username` (`username`),
  KEY `idx_users_status` (`status`),
  KEY `idx_users_username_lower` (`username_lower`),
  CONSTRAINT `fk_users_status` FOREIGN KEY (`status`) REFERENCES `statuses` (`code`),
  CONSTRAINT `chk_username_format` CHECK (`username` regexp '^[A-Za-z0-9._@-]{3,100}$')
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_uca1400_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `users`
--

LOCK TABLES `users` WRITE;
/*!40000 ALTER TABLE `users` DISABLE KEYS */;
INSERT INTO `users` VALUES
('5ba80dcd-23c0-11f1-b6ca-1cce51c6125a','admin1','admin1','active','es_MX',NULL,'2026-03-19 18:20:25','2026-03-19 18:20:25',NULL);
/*!40000 ALTER TABLE `users` ENABLE KEYS */;
UNLOCK TABLES;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_uca1400_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'IGNORE_SPACE,STRICT_TRANS_TABLES,ERROR_FOR_DIVISION_BY_ZERO,NO_AUTO_CREATE_USER,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
/*!50003 CREATE*/ /*!50017 DEFINER=`root`@`localhost`*/ /*!50003 TRIGGER trg_audit_users_insert AFTER INSERT ON users FOR EACH ROW BEGIN INSERT INTO audit_logs (id, table_name, record_id, action, performed_by, changed_data) VALUES (UUID(), 'users', NEW.id, 'INSERT', @app_current_user, JSON_OBJECT('new', JSON_OBJECT('id', NEW.id, 'username', NEW.username, 'status', NEW.status, 'created_at', NEW.created_at))); END */;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_uca1400_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'IGNORE_SPACE,STRICT_TRANS_TABLES,ERROR_FOR_DIVISION_BY_ZERO,NO_AUTO_CREATE_USER,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
/*!50003 CREATE*/ /*!50017 DEFINER=`root`@`localhost`*/ /*!50003 TRIGGER trg_audit_users_update AFTER UPDATE ON users FOR EACH ROW BEGIN INSERT INTO audit_logs (id, table_name, record_id, action, performed_by, changed_data) VALUES (UUID(), 'users', NEW.id, 'UPDATE', @app_current_user, JSON_OBJECT('old', JSON_OBJECT('username', OLD.username, 'status', OLD.status), 'new', JSON_OBJECT('username', NEW.username, 'status', NEW.status))); END */;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_uca1400_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'IGNORE_SPACE,STRICT_TRANS_TABLES,ERROR_FOR_DIVISION_BY_ZERO,NO_AUTO_CREATE_USER,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
/*!50003 CREATE*/ /*!50017 DEFINER=`root`@`localhost`*/ /*!50003 TRIGGER trg_audit_users_delete AFTER DELETE ON users FOR EACH ROW BEGIN INSERT INTO audit_logs (id, table_name, record_id, action, performed_by, changed_data) VALUES (UUID(), 'users', OLD.id, 'DELETE', @app_current_user, JSON_OBJECT('old', JSON_OBJECT('username', OLD.username, 'status', OLD.status))); END */;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;

--
-- Dumping routines for database 'library_mgmt'
--
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'IGNORE_SPACE,STRICT_TRANS_TABLES,ERROR_FOR_DIVISION_BY_ZERO,NO_AUTO_CREATE_USER,NO_ENGINE_SUBSTITUTION' */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_create_user` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_uca1400_ai_ci */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_create_user`(IN p_username VARCHAR(100),IN p_password_hash VARCHAR(255),IN p_first_name VARCHAR(100),IN p_last_name VARCHAR(100),IN p_contacts JSON,IN p_role_codes JSON,OUT out_user_id CHAR(36))
BEGIN DECLARE v_user_id CHAR(36); DECLARE v_role_id CHAR(36); DECLARE v_contact JSON; DECLARE v_idx INT DEFAULT 0; DECLARE v_len INT DEFAULT 0; START TRANSACTION; IF CHAR_LENGTH(TRIM(p_username)) < 3 THEN SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Username too short'; END IF; SET v_user_id = UUID(); INSERT INTO users (id, username, status, metadata) VALUES (v_user_id, p_username, 'active', NULL); INSERT INTO user_profiles (user_id, first_name, last_name, display_name) VALUES (v_user_id, p_first_name, p_last_name, CONCAT(p_first_name, ' ', p_last_name)); INSERT INTO user_auth (user_id, password_hash, password_changed_at) VALUES (v_user_id, p_password_hash, NOW()); IF p_contacts IS NOT NULL THEN SET v_len = JSON_LENGTH(p_contacts); SET v_idx = 0; WHILE v_idx < v_len DO SET v_contact = JSON_EXTRACT(p_contacts, CONCAT('$[', v_idx, ']')); INSERT INTO user_contacts (id, user_id, type, value, is_primary, is_verified) VALUES (UUID(), v_user_id, JSON_UNQUOTE(JSON_EXTRACT(v_contact, '$.type')), JSON_UNQUOTE(JSON_EXTRACT(v_contact, '$.value')), IF(JSON_EXTRACT(v_contact, '$.is_primary') = true, 1, 0), 0); SET v_idx = v_idx + 1; END WHILE; END IF; IF p_role_codes IS NOT NULL THEN SET v_len = JSON_LENGTH(p_role_codes); SET v_idx = 0; WHILE v_idx < v_len DO SET v_role_id = (SELECT id FROM roles WHERE code = JSON_UNQUOTE(JSON_EXTRACT(p_role_codes, CONCAT('$[', v_idx, ']'))) LIMIT 1); IF v_role_id IS NOT NULL THEN INSERT INTO user_roles (user_id, role_id, assigned_at) VALUES (v_user_id, v_role_id, NOW()); END IF; SET v_idx = v_idx + 1; END WHILE; END IF; COMMIT; SET out_user_id = v_user_id; END ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'IGNORE_SPACE,STRICT_TRANS_TABLES,ERROR_FOR_DIVISION_BY_ZERO,NO_AUTO_CREATE_USER,NO_ENGINE_SUBSTITUTION' */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_deactivate_user` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_uca1400_ai_ci */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_deactivate_user`(IN p_user_id CHAR(36),IN p_performed_by CHAR(36),IN p_reason TEXT)
BEGIN START TRANSACTION; UPDATE users SET status = 'deleted', deleted_at = NOW() WHERE id = p_user_id AND deleted_at IS NULL; INSERT INTO audit_logs (id, table_name, record_id, action, performed_by, changed_data, reason) VALUES (UUID(), 'users', p_user_id, 'UPDATE', p_performed_by, JSON_OBJECT('status','deleted','deleted_at',NOW()), p_reason); COMMIT; END ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'IGNORE_SPACE,STRICT_TRANS_TABLES,ERROR_FOR_DIVISION_BY_ZERO,NO_AUTO_CREATE_USER,NO_ENGINE_SUBSTITUTION' */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_get_book` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_uca1400_ai_ci */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_get_book`(
  IN p_book_id CHAR(36),
  IN p_isbn VARCHAR(50)
)
BEGIN
  SELECT
    b.id,
    b.title,
    b.subtitle,
    b.isbn,
    b.publisher,
    b.publication_year,
    b.edition,
    b.language,
    b.summary,
    b.metadata,
    GROUP_CONCAT(DISTINCT a.full_name SEPARATOR '; ') AS authors,
    GROUP_CONCAT(DISTINCT s.name SEPARATOR '; ') AS subjects
  FROM books b
  LEFT JOIN book_authors ba ON ba.book_id = b.id
  LEFT JOIN authors a ON a.id = ba.author_id
  LEFT JOIN book_subjects bs ON bs.book_id = b.id
  LEFT JOIN subjects s ON s.id = bs.subject_id
  WHERE (p_book_id IS NOT NULL AND p_book_id <> '' AND b.id = p_book_id)
     OR (p_isbn IS NOT NULL AND p_isbn <> '' AND b.isbn = p_isbn)
  GROUP BY b.id;

  SELECT
    c.id AS copy_id,
    c.barcode,
    c.accession_number,
    c.status,
    c.condition,
    l.library_name,
    l.section,
    l.shelf
  FROM book_copies c
  LEFT JOIN locations l ON l.id = c.location_id
  WHERE c.book_id = (SELECT id FROM books WHERE (p_book_id IS NOT NULL AND p_book_id <> '' AND id = p_book_id) OR (p_isbn IS NOT NULL AND p_isbn <> '' AND isbn = p_isbn) LIMIT 1);
END ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'IGNORE_SPACE,STRICT_TRANS_TABLES,ERROR_FOR_DIVISION_BY_ZERO,NO_AUTO_CREATE_USER,NO_ENGINE_SUBSTITUTION' */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_list_books` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_uca1400_ai_ci */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_list_books`(
  IN p_title VARCHAR(500),
  IN p_author VARCHAR(255),
  IN p_isbn VARCHAR(50),
  IN p_subject VARCHAR(255)
)
BEGIN
  SELECT DISTINCT
    b.id,
    b.title,
    b.subtitle,
    b.isbn,
    b.publisher,
    b.publication_year,
    b.edition,
    b.language,
    JSON_UNQUOTE(JSON_EXTRACT(b.summary, '$.short')) AS short_summary,
    GROUP_CONCAT(DISTINCT a.full_name SEPARATOR '; ') AS authors,
    GROUP_CONCAT(DISTINCT s.name SEPARATOR '; ') AS subjects
  FROM books b
  LEFT JOIN book_authors ba ON ba.book_id = b.id
  LEFT JOIN authors a ON a.id = ba.author_id
  LEFT JOIN book_subjects bs ON bs.book_id = b.id
  LEFT JOIN subjects s ON s.id = bs.subject_id
  WHERE (p_title IS NULL OR p_title = '' OR b.title LIKE CONCAT('%', p_title, '%'))
    AND (p_isbn IS NULL OR p_isbn = '' OR b.isbn = p_isbn)
    AND (p_author IS NULL OR p_author = '' OR a.full_name LIKE CONCAT('%', p_author, '%'))
    AND (p_subject IS NULL OR p_subject = '' OR s.name LIKE CONCAT('%', p_subject, '%'))
  GROUP BY b.id
  ORDER BY b.title;
END ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'IGNORE_SPACE,STRICT_TRANS_TABLES,ERROR_FOR_DIVISION_BY_ZERO,NO_AUTO_CREATE_USER,NO_ENGINE_SUBSTITUTION' */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_update_book` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_uca1400_ai_ci */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_update_book`(
  IN p_book_id CHAR(36),
  IN p_title VARCHAR(500),
  IN p_subtitle VARCHAR(500),
  IN p_isbn VARCHAR(50),
  IN p_publisher VARCHAR(255),
  IN p_publication_year SMALLINT,
  IN p_edition VARCHAR(100),
  IN p_language VARCHAR(50),
  IN p_summary JSON,
  IN p_metadata JSON,
  IN p_performed_by CHAR(36)
)
BEGIN
  START TRANSACTION;
  UPDATE books
    SET title = COALESCE(p_title, title),
        subtitle = COALESCE(p_subtitle, subtitle),
        isbn = COALESCE(p_isbn, isbn),
        publisher = COALESCE(p_publisher, publisher),
        publication_year = COALESCE(p_publication_year, publication_year),
        edition = COALESCE(p_edition, edition),
        language = COALESCE(p_language, language),
        summary = COALESCE(p_summary, summary),
        metadata = COALESCE(p_metadata, metadata),
        updated_at = NOW()
  WHERE id = p_book_id;
  INSERT INTO audit_logs (id, table_name, record_id, action, performed_by, changed_data)
    VALUES (UUID(), 'books', p_book_id, 'UPDATE', p_performed_by, JSON_OBJECT('update_payload', JSON_OBJECT('title', p_title, 'isbn', p_isbn)));
  COMMIT;
END ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'IGNORE_SPACE,STRICT_TRANS_TABLES,ERROR_FOR_DIVISION_BY_ZERO,NO_AUTO_CREATE_USER,NO_ENGINE_SUBSTITUTION' */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_update_user_profile` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_uca1400_ai_ci */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_update_user_profile`(IN p_user_id CHAR(36),IN p_profile JSON,IN p_performed_by CHAR(36))
BEGIN START TRANSACTION; UPDATE user_profiles SET first_name = COALESCE(JSON_UNQUOTE(JSON_EXTRACT(p_profile,'$.first_name')), first_name), last_name = COALESCE(JSON_UNQUOTE(JSON_EXTRACT(p_profile,'$.last_name')), last_name), display_name = COALESCE(JSON_UNQUOTE(JSON_EXTRACT(p_profile,'$.display_name')), display_name), address = COALESCE(JSON_EXTRACT(p_profile,'$.address'), address), updated_at = NOW() WHERE user_id = p_user_id; INSERT INTO audit_logs (id, table_name, record_id, action, performed_by, changed_data) VALUES (UUID(), 'user_profiles', p_user_id, 'UPDATE', p_performed_by, JSON_OBJECT('profile_update', p_profile)); COMMIT; END ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*M!100616 SET NOTE_VERBOSITY=@OLD_NOTE_VERBOSITY */;

-- Dump completed on 2026-03-22 13:11:11
