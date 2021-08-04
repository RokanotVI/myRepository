-- MySQL Workbench Forward Engineering

SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0;
SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0;
SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='TRADITIONAL,ALLOW_INVALID_DATES';

-- -----------------------------------------------------
-- Schema DBForTheTestTask
-- -----------------------------------------------------

-- -----------------------------------------------------
-- Schema DBForTheTestTask
-- -----------------------------------------------------
CREATE SCHEMA IF NOT EXISTS `DBForTheTestTask` DEFAULT CHARACTER SET utf8 ;
USE `DBForTheTestTask` ;

-- -----------------------------------------------------
-- Table `DBForTheTestTask`.`Employees`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `DBForTheTestTask`.`Employees` (
  `EmployeeID` INT NOT NULL AUTO_INCREMENT,
  `EmployeeName` VARCHAR(100) NULL,
  PRIMARY KEY (`EmployeeID`))
ENGINE = InnoDB;


-- -----------------------------------------------------
-- Table `DBForTheTestTask`.`Phones`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `DBForTheTestTask`.`Phones` (
  `PhoneID` INT NOT NULL AUTO_INCREMENT,
  `PhoneNumber` VARCHAR(14) NULL,
  `EmployeeID` INT NULL,
  PRIMARY KEY (`PhoneID`),
  INDEX `EmloyeeID_idx` (`EmployeeID` ASC),
  CONSTRAINT `EmloyeeID`
    FOREIGN KEY (`EmployeeID`)
    REFERENCES `DBForTheTestTask`.`Employees` (`EmployeeID`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION)
ENGINE = InnoDB;


-- -----------------------------------------------------
-- Table `DBForTheTestTask`.`DisabledPerson`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `DBForTheTestTask`.`DisabledPerson` (
  `DisabledPersonID` INT NOT NULL AUTO_INCREMENT,
  `EmployeeID` INT NOT NULL,
  PRIMARY KEY (`DisabledPersonID`),
  INDEX `DisabledPersonEmployeeID_idx` (`EmployeeID` ASC),
  UNIQUE INDEX `EmployeeID_UNIQUE` (`EmployeeID` ASC),
  CONSTRAINT `DisabledPersonEmployeeID`
    FOREIGN KEY (`EmployeeID`)
    REFERENCES `DBForTheTestTask`.`Employees` (`EmployeeID`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION)
ENGINE = InnoDB;

USE `DBForTheTestTask` ;

-- -----------------------------------------------------
-- procedure SelectAllEmployee
-- -----------------------------------------------------

DELIMITER $$
USE `DBForTheTestTask`$$
CREATE PROCEDURE `SelectAllEmployee` ()
BEGIN
select * from Employee;
END$$

DELIMITER ;

-- -----------------------------------------------------
-- procedure SelectEmployee
-- -----------------------------------------------------

DELIMITER $$
USE `DBForTheTestTask`$$
CREATE PROCEDURE `SelectEmployee` (IN emp VARCHAR(100))
BEGIN
Select * from Employee where EmployeeName = emp;
END$$

DELIMITER ;

SET SQL_MODE=@OLD_SQL_MODE;
SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS;
SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS;

-- -----------------------------------------------------
-- Data for table `DBForTheTestTask`.`Employees`
-- -----------------------------------------------------
START TRANSACTION;
USE `DBForTheTestTask`;
INSERT INTO `DBForTheTestTask`.`Employees` (`EmployeeID`, `EmployeeName`) VALUES (DEFAULT, 'Петя');
INSERT INTO `DBForTheTestTask`.`Employees` (`EmployeeID`, `EmployeeName`) VALUES (DEFAULT, 'Вася');
INSERT INTO `DBForTheTestTask`.`Employees` (`EmployeeID`, `EmployeeName`) VALUES (DEFAULT, 'Коля');
INSERT INTO `DBForTheTestTask`.`Employees` (`EmployeeID`, `EmployeeName`) VALUES (DEFAULT, 'Саша');

COMMIT;


-- -----------------------------------------------------
-- Data for table `DBForTheTestTask`.`Phones`
-- -----------------------------------------------------
START TRANSACTION;
USE `DBForTheTestTask`;
INSERT INTO `DBForTheTestTask`.`Phones` (`PhoneID`, `PhoneNumber`, `EmployeeID`) VALUES (DEFAULT, '9996664422', 1);
INSERT INTO `DBForTheTestTask`.`Phones` (`PhoneID`, `PhoneNumber`, `EmployeeID`) VALUES (DEFAULT, '6669994422', 1);
INSERT INTO `DBForTheTestTask`.`Phones` (`PhoneID`, `PhoneNumber`, `EmployeeID`) VALUES (DEFAULT, '4442229966', 2);
INSERT INTO `DBForTheTestTask`.`Phones` (`PhoneID`, `PhoneNumber`, `EmployeeID`) VALUES (DEFAULT, '2224449966', 4);
INSERT INTO `DBForTheTestTask`.`Phones` (`PhoneID`, `PhoneNumber`, `EmployeeID`) VALUES (DEFAULT, '9994446622', 4);

COMMIT;


-- -----------------------------------------------------
-- Data for table `DBForTheTestTask`.`DisabledPerson`
-- -----------------------------------------------------
START TRANSACTION;
USE `DBForTheTestTask`;
INSERT INTO `DBForTheTestTask`.`DisabledPerson` (`DisabledPersonID`, `EmployeeID`) VALUES (DEFAULT, 1);
INSERT INTO `DBForTheTestTask`.`DisabledPerson` (`DisabledPersonID`, `EmployeeID`) VALUES (DEFAULT, 4);

COMMIT;

