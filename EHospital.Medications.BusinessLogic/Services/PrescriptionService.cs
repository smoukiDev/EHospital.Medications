﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EHospital.Medications.BusinessLogic.Contracts;
using EHospital.Medications.Model;

namespace EHospital.Medications.BusinessLogic.Services
{
    /// <summary>Represents prescription service.</summary>
    /// <seealso cref="IPrescriptionService"/>
    public class PrescriptionService : IPrescriptionService
    {
        /// <summary>
        /// Exception message in case prescription with specified identifier isn't found.
        /// </summary>
        private const string PRESCRIPTION_IS_NOT_FOUND = "No prescription with such id.";

        /// <summary>
        /// Exception message in case any prescription was found in the database.
        /// </summary>
        private const string PRESCRIPTIONS_ARE_NOT_FOUND = "No prescriptions found by such patient id.";

        /// <summary>
        /// Combine usage of unit of work and repository pattern.
        /// It contain repositories for each entity
        /// and one common database context in order
        /// to access data and perform CRUD operation.
        /// Also contains required supplement operations
        /// which are not related to repositories.
        /// </summary>
        private readonly IUnitOfWork unitOfWork;

        /// <summary>
        /// Initializes a new instance of the <see cref="PrescriptionService"/> class.
        /// </summary>
        /// <param name="unitOfWork">The unit of work.</param>
        public PrescriptionService(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Creates entity in asynchronous mode.
        /// </summary>
        /// <param name="item">Entity to create.</param>
        /// <returns>
        /// Created entity.
        /// </returns>
        public async Task<Prescription> AddAsync(Prescription item)
        {
            Prescription result = this.unitOfWork.Prescriptions.Insert(item);
            await this.unitOfWork.Save();
            return result;
        }

        /// <summary>
        /// Deletes entity in asynchronous mode.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>
        /// Deleted entity.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// No prescription with such id.
        /// </exception>
        public async Task<Prescription> DeleteAsync(int id)
        {
            Prescription result = await this.unitOfWork.Prescriptions.DeleteAsync(id);
            if (result == null)
            {
                throw new ArgumentException(PRESCRIPTION_IS_NOT_FOUND);
            }

            await this.unitOfWork.Save();
            return result;
        }

        /// <summary>
        /// Gets entity by the identifier in asynchronous mode.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>
        /// Concrete entity.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// No prescription with such id.
        /// </exception>
        public async Task<Prescription> GetByIdAsync(int id)
        {
            Prescription result = await this.unitOfWork.Prescriptions.GetAsync(id);
            if (result == null || result.IsDeleted == true)
            {
                throw new ArgumentException(PRESCRIPTION_IS_NOT_FOUND);
            }

            return result;
        }

        /// <summary>
        /// Gets the guide by identifier in asynchronous mode.
        /// Includes drug instruction and doctor's notes.
        /// </summary>
        /// <param name="id">The prescription identifier.</param>
        /// <returns>
        /// Drug instruction and doctor's notes.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// No prescription with such id.
        /// </exception>
        public async Task<PrescriptionGuide> GetGuideById(int id)
        {
            // Return IQueryable<Prescription> with one entity
            var prescriptions = await this.unitOfWork.Prescriptions.GetAllAsync(p => p.Id == id && p.IsDeleted == false);
            if (prescriptions.Count() == 0)
            {
                throw new ArgumentException(PRESCRIPTION_IS_NOT_FOUND);
            }

            // Return IQueryable<Drug> with one entity
            var drugs = await this.unitOfWork.Drugs.GetAllAsync(d => d.Id == prescriptions.First().DrugId);

            // Return IQueryable<PrescriptionGuide> with one entity
            var guide = prescriptions.Join(
                drugs,
                p => p.DrugId,
                d => d.Id,
                (p, d) => new PrescriptionGuide
                {
                    Notes = p.Notes,
                    Instruction = d.Instruction
                });

            PrescriptionGuide result = guide.FirstOrDefault();

            return result;
        }

        /// <summary>
        /// Gets all prescription details specified by patient identifier
        /// in asynchronous mode. Includes doctor and drug extended details.
        /// </summary>
        /// <param name="patientId">The Patient identifier.</param>
        /// <returns>
        /// All patient prescriptions in details.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// No prescriptions found by such patient id.
        /// </exception>
        public async Task<IEnumerable<PrescriptionDetails>> GetPrescriptionsDetails(int patientId)
        {
            // Calculates CurrentDate - AssignmentDate, compares with Duration
            // and changes status to historic for all records where subtraction more the Duration
            await this.unitOfWork.UpdateStatusAutomatically();

            // Return IQueryable<Prescription> with prescription of concrete patient which are not deleted
            IQueryable<Prescription> prescriptions = await this.unitOfWork.Prescriptions
                .GetAllAsync(p => p.PatientId == patientId && p.IsDeleted == false);
            if (prescriptions.Count() == 0)
            {
                throw new ArgumentException(PRESCRIPTIONS_ARE_NOT_FOUND);
            }

            // Return IQueryable<Drug> with drugs
            Task<IQueryable<Drug>> drugs = this.unitOfWork.Drugs.GetAllAsync();

            // Return IQueryable<DoctorView> with drugs
            Task<IQueryable<DoctorView>> doctors = this.unitOfWork.GetAllDoctorsAsync();
            await Task.WhenAll(drugs, doctors);

            // Return IQueryable with one entity
            var details = from prescription in prescriptions
                          join drug in drugs.Result
                          on prescription.DrugId equals drug.Id
                          join doctor in doctors.Result
                          on prescription.UserId equals doctor.Id
                          select new PrescriptionDetails
                          {
                              Id = prescription.Id,
                              FirstName = doctor.FirstName,
                              LastName = doctor.LastName,
                              Name = drug.Name,
                              Type = drug.Type,
                              Dose = drug.Dose,
                              DoseUnit = drug.DoseUnit,
                              Direction = drug.Direction,
                              IsDeleted = drug.IsDeleted,
                              AssignmentDate = prescription.AssignmentDate,
                              Duration = prescription.Duration,
                              IsFinished = prescription.IsFinished
                          };

            return details.OrderBy(pd => pd.AssignmentDate);
        }

        /// <summary>
        /// Updates entity in asynchronous mode.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="item">The entity with updated properties.</param>
        /// <returns>
        /// Updated entity.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// No prescription with such id.
        /// </exception>
        public async Task<Prescription> UpdateAsync(int id, Prescription item)
        {
            Prescription result = await this.unitOfWork.Prescriptions.UpdateAsync(id, item);
            if (result == null || result.IsDeleted == true)
            {
                throw new ArgumentException(PRESCRIPTION_IS_NOT_FOUND);
            }

            await this.unitOfWork.Save();
            return result;
        }

        /// <summary>
        /// Allows to update prescription status manually to historic
        /// in asynchronous mode.
        /// </summary>
        /// <param name="id">The prescription identifier.</param>
        /// <returns>
        /// Historic prescription.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// No prescription with such id.
        /// </exception>
        public async Task<Prescription> UpdateStatusAsync(int id)
        {
            Prescription result = await this.unitOfWork.Prescriptions.GetAsync(id);
            if (result == null || result.IsDeleted == true || result.IsFinished == true)
            {
                throw new ArgumentException(PRESCRIPTION_IS_NOT_FOUND);
            }

            await this.unitOfWork.UpdateStatusManually(id);
            return await this.unitOfWork.Prescriptions.GetAsync(id);
        }
    }
}